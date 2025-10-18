# Python SDK

## Installation

```bash
pip install productinventory-api
```

## Basic Usage

```python
from productinventory_api import ProductInventoryAPI
from productinventory_api.models import CreateProductRequest, CreateInventoryRequest

# Initialize the API client
api = ProductInventoryAPI(base_url='https://api.productinventory.com/api/v1')

# Login and get token
login_result = api.auth.login('user@example.com', 'password123')
if login_result.success:
    print('Logged in successfully')
    # Token is automatically stored and used for subsequent requests
```

## Complete Examples

### Product Management

```python
from productinventory_api import ProductInventoryAPI
from productinventory_api.models import *
from productinventory_api.exceptions import APIError, ValidationError
import logging
from typing import List, Optional

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class ProductManager:
    def __init__(self, base_url: str):
        self.api = ProductInventoryAPI(base_url=base_url)
        self.authenticated = False

    async def initialize(self, email: str, password: str):
        """Initialize and authenticate the client"""
        try:
            result = await self.api.auth.login(email, password)
            if result.success:
                self.authenticated = True
                logger.info("Successfully authenticated")
            else:
                raise APIError(f"Authentication failed: {result.message}")
        except Exception as e:
            logger.error(f"Failed to initialize: {e}")
            raise

    async def create_product(self, product_data: CreateProductRequest) -> Optional[Product]:
        """Create a new product"""
        if not self.authenticated:
            raise RuntimeError("Client not authenticated")

        try:
            result = await self.api.products.create(product_data)
            
            if result.success:
                logger.info(f"Product created: {result.data.name} (ID: {result.data.id})")
                return result.data
            else:
                logger.error(f"Failed to create product: {result.message}")
                if result.errors:
                    for error in result.errors:
                        logger.error(f"  - {error.field}: {error.message}")
                return None
        except ValidationError as e:
            logger.error(f"Validation error: {e}")
            raise
        except APIError as e:
            logger.error(f"API error: {e}")
            raise

    async def get_products(self, 
                          category_id: Optional[int] = None,
                          min_price: Optional[float] = None,
                          max_price: Optional[float] = None,
                          in_stock: Optional[bool] = None,
                          search: Optional[str] = None,
                          page: int = 1,
                          page_size: int = 20) -> Optional[PaginatedResponse[Product]]:
        """Get products with filtering"""
        try:
            filters = ProductFilter(
                categoryId=category_id,
                minPrice=min_price,
                maxPrice=max_price,
                inStock=in_stock,
                search=search,
                page=page,
                pageSize=page_size
            )
            
            result = await self.api.products.get_all(filters)
            
            if result.success:
                logger.info(f"Retrieved {len(result.data)} products")
                return PaginatedResponse(
                    data=result.data,
                    pagination=result.pagination
                )
            else:
                logger.error(f"Failed to fetch products: {result.message}")
                return None
        except Exception as e:
            logger.error(f"Error fetching products: {e}")
            raise

    async def update_product_stock(self, 
                                  product_id: int, 
                                  quantity_change: int, 
                                  movement_type: str,
                                  reason: str,
                                  reference: Optional[str] = None) -> bool:
        """Update product stock level"""
        try:
            movement_request = CreateInventoryMovementRequest(
                productId=product_id,
                movementType=movement_type,
                quantity=quantity_change,
                notes=reason,
                reference=reference or f"STOCK-UPDATE-{product_id}-{int(time.time())}"
            )
            
            result = await self.api.inventory_movement.record(movement_request)
            
            if result.success:
                logger.info(f"Stock updated for product {product_id}: {quantity_change} units")
                return True
            else:
                logger.error(f"Failed to update stock: {result.message}")
                return False
        except Exception as e:
            logger.error(f"Error updating stock: {e}")
            raise

    async def get_low_stock_products(self) -> List[StockAlert]:
        """Get products with low stock levels"""
        try:
            result = await self.api.inventory.get_stock_alerts(level='low')
            
            if result.success:
                return result.data
            else:
                logger.error(f"Failed to fetch stock alerts: {result.message}")
                return []
        except Exception as e:
            logger.error(f"Error fetching stock alerts: {e}")
            raise

    async def get_product_analytics(self) -> dict:
        """Get comprehensive product analytics"""
        try:
            # Fetch all analytics data concurrently
            import asyncio
            
            category_stats_task = self.api.analytics.get_category_statistics()
            inventory_summary_task = self.api.inventory.get_summary()
            low_stock_task = self.api.inventory.get_stock_alerts(level='low')
            out_of_stock_task = self.api.inventory.get_stock_alerts(level='out')
            
            results = await asyncio.gather(
                category_stats_task,
                inventory_summary_task,
                low_stock_task,
                out_of_stock_task,
                return_exceptions=True
            )
            
            return {
                'category_statistics': results[0].data if results[0].success else [],
                'inventory_summary': results[1].data if results[1].success else None,
                'low_stock_alerts': results[2].data if results[2].success else [],
                'out_of_stock_alerts': results[3].data if results[3].success else []
            }
        except Exception as e:
            logger.error(f"Error fetching analytics: {e}")
            raise

# Usage example
async def main():
    import asyncio
    
    manager = ProductManager('https://api.productinventory.com/api/v1')
    
    try:
        # Initialize and authenticate
        await manager.initialize('manager@example.com', 'password123')
        
        # Create a new product
        new_product = await manager.create_product(
            CreateProductRequest(
                name='Python SDK Test Product',
                description='Product created using Python SDK',
                sku='PYTHON-001',
                price=299.99,
                categoryId=1,
                inventory=CreateInventoryRequest(
                    currentStock=100,
                    reorderPoint=20,
                    maxStockLevel=200
                )
            )
        )
        
        if new_product:
            print(f"Created product: {new_product.name}")
            
            # Update stock (simulate sale)
            await manager.update_product_stock(
                product_id=new_product.id,
                quantity_change=-5,
                movement_type='Sale',
                reason='Online order processing',
                reference='ORDER-PYTHON-001'
            )
            
            # Get products in same category
            products = await manager.get_products(
                category_id=new_product.categoryId,
                page=1,
                page_size=10
            )
            
            if products:
                print(f"Found {len(products.data)} products in category")
            
            # Get analytics
            analytics = await manager.get_product_analytics()
            print("Analytics summary:")
            if analytics['inventory_summary']:
                summary = analytics['inventory_summary']
                print(f"  Total products: {summary.totalProducts}")
                print(f"  Total value: ${summary.totalValue:.2f}")
                print(f"  Low stock items: {summary.lowStockItems}")
        
    except Exception as e:
        logger.error(f"Application error: {e}")

# Run the example
if __name__ == "__main__":
    asyncio.run(main())
```

### Inventory Monitoring System

```python
import asyncio
import time
import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
from typing import List, Dict, Optional
from productinventory_api import ProductInventoryAPI
from productinventory_api.models import StockAlert
import logging

logger = logging.getLogger(__name__)

class InventoryMonitoringSystem:
    def __init__(self, api_base_url: str, monitoring_interval: int = 300):
        self.api = ProductInventoryAPI(base_url=api_base_url)
        self.monitoring_interval = monitoring_interval  # seconds
        self.is_monitoring = False
        self.alert_history: Dict[int, StockAlert] = {}
        
        # Email configuration (optional)
        self.email_config = {
            'smtp_server': 'smtp.gmail.com',
            'smtp_port': 587,
            'username': '',
            'password': '',
            'recipients': []
        }

    async def initialize(self, email: str, password: str):
        """Initialize the monitoring system"""
        try:
            result = await self.api.auth.login(email, password)
            if not result.success:
                raise Exception(f"Authentication failed: {result.message}")
            logger.info("Inventory monitoring system initialized")
        except Exception as e:
            logger.error(f"Failed to initialize: {e}")
            raise

    async def start_monitoring(self):
        """Start the inventory monitoring loop"""
        self.is_monitoring = True
        logger.info(f"Starting inventory monitoring (interval: {self.monitoring_interval}s)")
        
        while self.is_monitoring:
            try:
                await self.check_inventory_levels()
                await asyncio.sleep(self.monitoring_interval)
            except Exception as e:
                logger.error(f"Error during monitoring cycle: {e}")
                await asyncio.sleep(60)  # Wait 1 minute before retrying

    def stop_monitoring(self):
        """Stop the monitoring loop"""
        self.is_monitoring = False
        logger.info("Inventory monitoring stopped")

    async def check_inventory_levels(self):
        """Check current inventory levels and generate alerts"""
        try:
            # Get all stock alerts
            alerts_result = await self.api.inventory.get_stock_alerts()
            
            if not alerts_result.success:
                logger.error(f"Failed to fetch stock alerts: {alerts_result.message}")
                return

            current_alerts = alerts_result.data
            logger.info(f"Found {len(current_alerts)} stock alerts")

            # Process each alert
            for alert in current_alerts:
                await self.process_alert(alert)

            # Get inventory summary for reporting
            summary_result = await self.api.inventory.get_summary()
            if summary_result.success:
                summary = summary_result.data
                logger.info(
                    f"Inventory Summary: {summary.totalProducts} products, "
                    f"${summary.totalValue:.2f} total value, "
                    f"{summary.lowStockItems} low stock, "
                    f"{summary.outOfStockItems} out of stock"
                )

        except Exception as e:
            logger.error(f"Error checking inventory levels: {e}")

    async def process_alert(self, alert: StockAlert):
        """Process individual stock alert"""
        try:
            alert_key = alert.productId
            previous_alert = self.alert_history.get(alert_key)
            
            # Check if this is a new alert or status change
            is_new_alert = (
                previous_alert is None or 
                previous_alert.alertLevel != alert.alertLevel or
                previous_alert.currentStock != alert.currentStock
            )
            
            if is_new_alert:
                self.alert_history[alert_key] = alert
                
                if alert.alertLevel == 'out':
                    await self.handle_out_of_stock(alert)
                elif alert.alertLevel == 'low':
                    await self.handle_low_stock(alert)
                elif alert.alertLevel == 'overstocked':
                    await self.handle_overstocked(alert)
                    
        except Exception as e:
            logger.error(f"Error processing alert for product {alert.productId}: {e}")

    async def handle_out_of_stock(self, alert: StockAlert):
        """Handle out of stock alert"""
        message = (
            f"🚨 CRITICAL: Product '{alert.productName}' (SKU: {alert.sku}) is OUT OF STOCK!\n"
            f"Immediate action required to restock this item."
        )
        
        logger.critical(message)
        await self.send_notification(
            subject=f"URGENT: Out of Stock - {alert.productName}",
            message=message,
            priority='critical'
        )
        
        # Optionally, automatically create reorder suggestion
        await self.suggest_reorder(alert)

    async def handle_low_stock(self, alert: StockAlert):
        """Handle low stock alert"""
        days_info = f" (Est. {alert.daysUntilStockout} days until stockout)" if alert.daysUntilStockout else ""
        message = (
            f"⚠️ WARNING: Product '{alert.productName}' (SKU: {alert.sku}) is running low!\n"
            f"Current stock: {alert.currentStock}, Reorder point: {alert.reorderPoint}{days_info}\n"
            f"Consider reordering soon."
        )
        
        logger.warning(message)
        await self.send_notification(
            subject=f"Low Stock Warning - {alert.productName}",
            message=message,
            priority='warning'
        )

    async def handle_overstocked(self, alert: StockAlert):
        """Handle overstocked alert"""
        message = (
            f"📦 INFO: Product '{alert.productName}' (SKU: {alert.sku}) is overstocked.\n"
            f"Current stock: {alert.currentStock}, Max level: {alert.maxStockLevel}\n"
            f"Consider running promotions or adjusting reorder levels."
        )
        
        logger.info(message)
        await self.send_notification(
            subject=f"Overstocked - {alert.productName}",
            message=message,
            priority='info'
        )

    async def suggest_reorder(self, alert: StockAlert):
        """Suggest reorder quantity for out of stock items"""
        try:
            # Calculate suggested reorder quantity
            suggested_quantity = alert.maxStockLevel - alert.currentStock
            
            logger.info(
                f"Reorder suggestion for {alert.productName}: "
                f"Order {suggested_quantity} units to reach max stock level"
            )
            
            # You could integrate with purchasing system here
            # await self.create_purchase_order(alert.productId, suggested_quantity)
            
        except Exception as e:
            logger.error(f"Error creating reorder suggestion: {e}")

    async def send_notification(self, subject: str, message: str, priority: str = 'info'):
        """Send notification via email, Slack, etc."""
        try:
            # Log the notification
            logger.info(f"[{priority.upper()}] {subject}: {message}")
            
            # Send email if configured
            if self.email_config['recipients'] and self.email_config['username']:
                await self.send_email_notification(subject, message)
            
            # You can add other notification methods here:
            # - Slack webhook
            # - SMS via Twilio
            # - Push notifications
            # - Discord webhook
            # etc.
            
        except Exception as e:
            logger.error(f"Error sending notification: {e}")

    async def send_email_notification(self, subject: str, message: str):
        """Send email notification"""
        try:
            msg = MIMEMultipart()
            msg['From'] = self.email_config['username']
            msg['To'] = ', '.join(self.email_config['recipients'])
            msg['Subject'] = f"[Inventory Alert] {subject}"
            
            body = f"""
            Inventory Management System Alert
            
            {message}
            
            Timestamp: {time.strftime('%Y-%m-%d %H:%M:%S')}
            
            This is an automated message from the Product Inventory API monitoring system.
            """
            
            msg.attach(MIMEText(body, 'plain'))
            
            server = smtplib.SMTP(self.email_config['smtp_server'], self.email_config['smtp_port'])
            server.starttls()
            server.login(self.email_config['username'], self.email_config['password'])
            server.send_message(msg)
            server.quit()
            
            logger.info(f"Email notification sent: {subject}")
            
        except Exception as e:
            logger.error(f"Failed to send email notification: {e}")

    def configure_email(self, smtp_server: str, smtp_port: int, username: str, 
                       password: str, recipients: List[str]):
        """Configure email notifications"""
        self.email_config.update({
            'smtp_server': smtp_server,
            'smtp_port': smtp_port,
            'username': username,
            'password': password,
            'recipients': recipients
        })

    async def generate_daily_report(self) -> str:
        """Generate daily inventory report"""
        try:
            # Get comprehensive inventory data
            summary_result = await self.api.inventory.get_summary()
            analytics_result = await self.api.analytics.get_category_statistics()
            alerts_result = await self.api.inventory.get_stock_alerts()
            
            if not all([summary_result.success, analytics_result.success, alerts_result.success]):
                return "Failed to generate daily report - API errors occurred"
            
            summary = summary_result.data
            analytics = analytics_result.data
            alerts = alerts_result.data
            
            report = f"""
            Daily Inventory Report - {time.strftime('%Y-%m-%d')}
            
            SUMMARY:
            ========
            Total Products: {summary.totalProducts}
            Total Inventory Value: ${summary.totalValue:,.2f}
            Low Stock Items: {summary.lowStockItems}
            Out of Stock Items: {summary.outOfStockItems}
            
            CATEGORY BREAKDOWN:
            ==================
            """
            
            for category in analytics:
                report += f"""
            {category.categoryName}:
              - Products: {category.productCount}
              - Value: ${category.totalInventoryValue:,.2f}
              - Low Stock: {category.lowStockProductCount}
              - Out of Stock: {category.outOfStockProductCount}
            """
            
            if alerts:
                report += f"""
            
            CURRENT ALERTS:
            ===============
            """
                for alert in alerts[:10]:  # Top 10 alerts
                    report += f"- {alert.productName} ({alert.sku}): {alert.alertLevel.upper()} - {alert.currentStock} units\n"
            
            return report
            
        except Exception as e:
            logger.error(f"Error generating daily report: {e}")
            return f"Error generating report: {e}"

# Usage example
async def monitoring_example():
    # Configure logging
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
    )
    
    # Initialize monitoring system
    monitor = InventoryMonitoringSystem(
        api_base_url='https://api.productinventory.com/api/v1',
        monitoring_interval=300  # Check every 5 minutes
    )
    
    # Configure email notifications (optional)
    monitor.configure_email(
        smtp_server='smtp.gmail.com',
        smtp_port=587,
        username='your-email@gmail.com',
        password='your-app-password',
        recipients=['manager@company.com', 'warehouse@company.com']
    )
    
    try:
        # Initialize and start monitoring
        await monitor.initialize('monitor@example.com', 'password123')
        
        # Generate initial report
        daily_report = await monitor.generate_daily_report()
        print("Daily Report:")
        print(daily_report)
        
        # Start monitoring (runs indefinitely)
        await monitor.start_monitoring()
        
    except KeyboardInterrupt:
        logger.info("Received interrupt signal")
        monitor.stop_monitoring()
    except Exception as e:
        logger.error(f"Monitoring system error: {e}")

if __name__ == "__main__":
    asyncio.run(monitoring_example())
```

### Data Analytics and Reporting

```python
import asyncio
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from datetime import datetime, timedelta
from typing import List, Dict, Optional
from productinventory_api import ProductInventoryAPI
from productinventory_api.models import *
import logging

logger = logging.getLogger(__name__)

class InventoryAnalytics:
    def __init__(self, api_base_url: str):
        self.api = ProductInventoryAPI(base_url=api_base_url)

    async def initialize(self, email: str, password: str):
        """Initialize the analytics system"""
        result = await self.api.auth.login(email, password)
        if not result.success:
            raise Exception(f"Authentication failed: {result.message}")

    async def collect_comprehensive_data(self) -> Dict:
        """Collect all inventory data for analysis"""
        try:
            # Collect data from multiple endpoints
            tasks = [
                self.api.products.get_all(ProductFilter(pageSize=1000)),
                self.api.categories.get_all(),
                self.api.analytics.get_category_statistics(),
                self.api.inventory.get_summary(),
                self.api.inventory.get_stock_alerts()
            ]
            
            results = await asyncio.gather(*tasks)
            
            return {
                'products': results[0].data if results[0].success else [],
                'categories': results[1].data if results[1].success else [],
                'category_stats': results[2].data if results[2].success else [],
                'inventory_summary': results[3].data if results[3].success else None,
                'alerts': results[4].data if results[4].success else []
            }
        except Exception as e:
            logger.error(f"Error collecting data: {e}")
            raise

    def create_products_dataframe(self, products: List[Product]) -> pd.DataFrame:
        """Convert products to pandas DataFrame"""
        data = []
        for product in products:
            data.append({
                'product_id': product.id,
                'name': product.name,
                'sku': product.sku,
                'price': product.price,
                'category_id': product.categoryId,
                'category_name': product.category.name if product.category else 'Unknown',
                'current_stock': product.inventory.currentStock if product.inventory else 0,
                'reorder_point': product.inventory.reorderPoint if product.inventory else 0,
                'max_stock_level': product.inventory.maxStockLevel if product.inventory else 0,
                'is_in_stock': product.inventory.isInStock if product.inventory else False,
                'stock_value': product.price * (product.inventory.currentStock if product.inventory else 0),
                'is_active': product.isActive,
                'created_at': product.createdAt,
                'updated_at': product.updatedAt
            })
        
        return pd.DataFrame(data)

    def analyze_inventory_health(self, df: pd.DataFrame) -> Dict:
        """Analyze overall inventory health"""
        total_products = len(df)
        active_products = len(df[df['is_active']])
        total_value = df['stock_value'].sum()
        
        # Stock level analysis
        in_stock = len(df[df['is_in_stock']])
        low_stock = len(df[df['current_stock'] <= df['reorder_point']])
        out_of_stock = len(df[df['current_stock'] == 0])
        overstocked = len(df[df['current_stock'] > df['max_stock_level']])
        
        # Category distribution
        category_distribution = df.groupby('category_name').agg({
            'product_id': 'count',
            'stock_value': 'sum',
            'current_stock': 'sum'
        }).round(2)
        
        # Price analysis
        price_stats = df['price'].describe()
        
        # Stock level distribution
        stock_distribution = df['current_stock'].describe()
        
        return {
            'overview': {
                'total_products': total_products,
                'active_products': active_products,
                'total_value': round(total_value, 2),
                'in_stock_percentage': round((in_stock / total_products) * 100, 2),
                'low_stock_count': low_stock,
                'out_of_stock_count': out_of_stock,
                'overstocked_count': overstocked
            },
            'category_distribution': category_distribution.to_dict(),
            'price_statistics': price_stats.to_dict(),
            'stock_statistics': stock_distribution.to_dict()
        }

    def identify_trends_and_insights(self, df: pd.DataFrame) -> List[str]:
        """Identify key trends and insights"""
        insights = []
        
        # High-value low-stock items
        high_value_low_stock = df[
            (df['stock_value'] > df['stock_value'].quantile(0.8)) & 
            (df['current_stock'] <= df['reorder_point'])
        ]
        
        if not high_value_low_stock.empty:
            insights.append(
                f"⚠️ {len(high_value_low_stock)} high-value products are running low on stock, "
                f"representing ${high_value_low_stock['stock_value'].sum():.2f} in inventory value at risk."
            )
        
        # Overstocked analysis
        overstocked = df[df['current_stock'] > df['max_stock_level']]
        if not overstocked.empty:
            excess_value = overstocked['stock_value'].sum()
            insights.append(
                f"📦 {len(overstocked)} products are overstocked, tying up "
                f"${excess_value:.2f} in excess inventory."
            )
        
        # Category performance
        category_performance = df.groupby('category_name')['stock_value'].sum().sort_values(ascending=False)
        top_category = category_performance.index[0]
        top_value = category_performance.iloc[0]
        
        insights.append(
            f"🏆 '{top_category}' is the highest-value category with "
            f"${top_value:.2f} in inventory value."
        )
        
        # Price distribution insights
        expensive_products = df[df['price'] > df['price'].quantile(0.9)]
        insights.append(
            f"💰 Top 10% most expensive products ({len(expensive_products)} items) "
            f"represent ${expensive_products['stock_value'].sum():.2f} in inventory value."
        )
        
        # Stock turn recommendations
        zero_stock_active = df[(df['current_stock'] == 0) & (df['is_active'])]
        if not zero_stock_active.empty:
            insights.append(
                f"🔴 {len(zero_stock_active)} active products have zero stock and need immediate attention."
            )
        
        return insights

    def generate_visualizations(self, df: pd.DataFrame, output_dir: str = './reports'):
        """Generate visualization charts"""
        import os
        os.makedirs(output_dir, exist_ok=True)
        
        # Set style
        plt.style.use('seaborn-v0_8')
        
        # 1. Category distribution pie chart
        plt.figure(figsize=(10, 8))
        category_counts = df.groupby('category_name').size()
        plt.pie(category_counts.values, labels=category_counts.index, autopct='%1.1f%%')
        plt.title('Product Distribution by Category')
        plt.savefig(f'{output_dir}/category_distribution.png', bbox_inches='tight', dpi=300)
        plt.close()
        
        # 2. Stock level histogram
        plt.figure(figsize=(12, 6))
        plt.hist(df['current_stock'], bins=30, alpha=0.7, edgecolor='black')
        plt.axvline(df['current_stock'].mean(), color='red', linestyle='--', label=f'Mean: {df["current_stock"].mean():.1f}')
        plt.xlabel('Current Stock Level')
        plt.ylabel('Number of Products')
        plt.title('Distribution of Current Stock Levels')
        plt.legend()
        plt.savefig(f'{output_dir}/stock_distribution.png', bbox_inches='tight', dpi=300)
        plt.close()
        
        # 3. Price vs Stock scatter plot
        plt.figure(figsize=(12, 8))
        scatter = plt.scatter(df['price'], df['current_stock'], 
                            c=df['stock_value'], s=50, alpha=0.6, cmap='viridis')
        plt.colorbar(scatter, label='Stock Value ($)')
        plt.xlabel('Product Price ($)')
        plt.ylabel('Current Stock Level')
        plt.title('Price vs Stock Level (colored by stock value)')
        plt.savefig(f'{output_dir}/price_vs_stock.png', bbox_inches='tight', dpi=300)
        plt.close()
        
        # 4. Category value breakdown
        plt.figure(figsize=(14, 8))
        category_values = df.groupby('category_name')['stock_value'].sum().sort_values(ascending=True)
        category_values.plot(kind='barh')
        plt.xlabel('Total Stock Value ($)')
        plt.title('Inventory Value by Category')
        plt.tight_layout()
        plt.savefig(f'{output_dir}/category_values.png', bbox_inches='tight', dpi=300)
        plt.close()
        
        # 5. Stock status overview
        plt.figure(figsize=(10, 6))
        stock_status = {
            'In Stock': len(df[df['is_in_stock'] & (df['current_stock'] > df['reorder_point'])]),
            'Low Stock': len(df[df['current_stock'] <= df['reorder_point']]),
            'Out of Stock': len(df[df['current_stock'] == 0]),
            'Overstocked': len(df[df['current_stock'] > df['max_stock_level']])
        }
        
        colors = ['green', 'orange', 'red', 'blue']
        plt.bar(stock_status.keys(), stock_status.values(), color=colors, alpha=0.7)
        plt.ylabel('Number of Products')
        plt.title('Product Count by Stock Status')
        plt.xticks(rotation=45)
        plt.tight_layout()
        plt.savefig(f'{output_dir}/stock_status.png', bbox_inches='tight', dpi=300)
        plt.close()
        
        logger.info(f"Visualization charts saved to {output_dir}")

    async def generate_comprehensive_report(self, output_dir: str = './reports') -> str:
        """Generate comprehensive inventory analysis report"""
        try:
            # Collect data
            logger.info("Collecting inventory data...")
            data = await self.collect_comprehensive_data()
            
            # Create DataFrame
            df = self.create_products_dataframe(data['products'])
            
            # Perform analysis
            logger.info("Analyzing inventory health...")
            health_analysis = self.analyze_inventory_health(df)
            insights = self.identify_trends_and_insights(df)
            
            # Generate visualizations
            logger.info("Generating visualizations...")
            self.generate_visualizations(df, output_dir)
            
            # Create report
            report = f"""
COMPREHENSIVE INVENTORY ANALYSIS REPORT
Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

EXECUTIVE SUMMARY
=================
Total Products: {health_analysis['overview']['total_products']}
Active Products: {health_analysis['overview']['active_products']}
Total Inventory Value: ${health_analysis['overview']['total_value']:,.2f}
Products In Stock: {health_analysis['overview']['in_stock_percentage']}%

STOCK STATUS BREAKDOWN
======================
Low Stock Items: {health_analysis['overview']['low_stock_count']}
Out of Stock Items: {health_analysis['overview']['out_of_stock_count']}
Overstocked Items: {health_analysis['overview']['overstocked_count']}

KEY INSIGHTS
============
"""
            
            for i, insight in enumerate(insights, 1):
                report += f"{i}. {insight}\n"
            
            report += f"""

CATEGORY PERFORMANCE
====================
"""
            
            for category, stats in health_analysis['category_distribution'].items():
                report += f"""
{category}:
  Products: {stats['product_id']}
  Total Value: ${stats['stock_value']:,.2f}
  Total Stock: {stats['current_stock']:,.0f} units
"""
            
            report += f"""

STATISTICAL SUMMARY
===================
Price Statistics:
  Mean: ${health_analysis['price_statistics']['mean']:.2f}
  Median: ${health_analysis['price_statistics']['50%']:.2f}
  Std Dev: ${health_analysis['price_statistics']['std']:.2f}
  Min: ${health_analysis['price_statistics']['min']:.2f}
  Max: ${health_analysis['price_statistics']['max']:.2f}

Stock Level Statistics:
  Mean: {health_analysis['stock_statistics']['mean']:.1f} units
  Median: {health_analysis['stock_statistics']['50%']:.1f} units
  Std Dev: {health_analysis['stock_statistics']['std']:.1f} units
  Min: {health_analysis['stock_statistics']['min']:.0f} units
  Max: {health_analysis['stock_statistics']['max']:.0f} units

RECOMMENDATIONS
===============
1. Reorder products with low stock levels immediately
2. Consider promotions for overstocked items
3. Review reorder points for frequently out-of-stock items
4. Analyze slow-moving high-value inventory
5. Implement automated reordering for critical items

CHARTS AND VISUALIZATIONS
=========================
The following charts have been generated and saved to {output_dir}:
- category_distribution.png: Product distribution by category
- stock_distribution.png: Histogram of stock levels
- price_vs_stock.png: Price vs stock relationship
- category_values.png: Inventory value by category
- stock_status.png: Product count by stock status

Report generated by Product Inventory API Analytics System
"""
            
            # Save report to file
            import os
            os.makedirs(output_dir, exist_ok=True)
            report_file = f"{output_dir}/inventory_analysis_report.txt"
            
            with open(report_file, 'w') as f:
                f.write(report)
            
            logger.info(f"Comprehensive report saved to {report_file}")
            return report
            
        except Exception as e:
            logger.error(f"Error generating report: {e}")
            raise

# Usage example
async def analytics_example():
    # Configure logging
    logging.basicConfig(level=logging.INFO)
    
    # Initialize analytics system
    analytics = InventoryAnalytics('https://api.productinventory.com/api/v1')
    
    try:
        # Initialize and authenticate
        await analytics.initialize('analyst@example.com', 'password123')
        
        # Generate comprehensive report
        report = await analytics.generate_comprehensive_report('./inventory_reports')
        
        print("=" * 80)
        print("INVENTORY ANALYSIS COMPLETE")
        print("=" * 80)
        print(report[:1000] + "..." if len(report) > 1000 else report)
        
    except Exception as e:
        logger.error(f"Analytics error: {e}")

if __name__ == "__main__":
    asyncio.run(analytics_example())
```

## Error Handling

```python
from productinventory_api.exceptions import (
    APIError, 
    ValidationError, 
    AuthenticationError, 
    NotFoundError,
    RateLimitError
)

try:
    result = await api.products.create(invalid_product_data)
except ValidationError as e:
    print(f"Validation failed: {e}")
    for error in e.validation_errors:
        print(f"  {error.field}: {error.message}")
except AuthenticationError as e:
    print(f"Authentication failed: {e}")
    # Refresh token or re-login
except NotFoundError as e:
    print(f"Resource not found: {e}")
except RateLimitError as e:
    print(f"Rate limit exceeded. Retry after: {e.retry_after}")
except APIError as e:
    print(f"API error: {e}")
    print(f"Status code: {e.status_code}")
    print(f"Correlation ID: {e.correlation_id}")
```

## Configuration

```python
from productinventory_api import ProductInventoryAPI, Config

# Custom configuration
config = Config(
    timeout=30.0,  # Request timeout
    retries=3,     # Retry attempts
    retry_delay=1.0,  # Delay between retries
    user_agent='MyApp/1.0.0'
)

api = ProductInventoryAPI(
    base_url='https://api.productinventory.com/api/v1',
    config=config
)

# Alternative: environment-based configuration
# Set environment variables:
# INVENTORY_API_BASE_URL=https://api.productinventory.com/api/v1
# INVENTORY_API_TIMEOUT=30
# INVENTORY_API_RETRIES=3

api = ProductInventoryAPI.from_environment()
```

## Async/Await Support

All SDK methods are async and use modern Python async/await patterns:

```python
import asyncio
from productinventory_api import ProductInventoryAPI

async def example():
    api = ProductInventoryAPI(base_url='https://api.productinventory.com/api/v1')
    
    # All methods are async
    await api.auth.login('user@example.com', 'password123')
    products = await api.products.get_all()
    analytics = await api.analytics.get_category_statistics()
    
    # Concurrent operations
    tasks = [
        api.products.get_by_id(1),
        api.products.get_by_id(2),
        api.products.get_by_id(3)
    ]
    results = await asyncio.gather(*tasks)

# Run async code
asyncio.run(example())
```

## Type Hints

The SDK includes comprehensive type hints for better IDE support:

```python
from typing import List, Optional
from productinventory_api.models import Product, Category, PaginatedResponse

async def get_products_by_category(
    api: ProductInventoryAPI, 
    category_id: int
) -> Optional[List[Product]]:
    
    result: PaginatedResponse[Product] = await api.products.get_all(
        ProductFilter(categoryId=category_id)
    )
    
    if result.success:
        return result.data
    return None
```