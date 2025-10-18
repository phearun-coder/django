# JavaScript/TypeScript SDK

## Installation

```bash
npm install productinventory-api
# or
yarn add productinventory-api
```

## Basic Usage

```typescript
import { ProductInventoryAPI } from 'productinventory-api';

const api = new ProductInventoryAPI({
  baseUrl: 'https://api.productinventory.com/api/v1',
  apiKey: 'your-api-key' // Optional for public endpoints
});

// Login and get token
const loginResult = await api.auth.login('user@example.com', 'password123');
if (loginResult.success) {
  console.log('Logged in successfully');
  // Token is automatically stored and used for subsequent requests
}
```

## Complete Examples

### Product Management

```typescript
import { ProductInventoryAPI, CreateProductRequest } from 'productinventory-api';

class ProductManager {
  private api: ProductInventoryAPI;

  constructor(baseUrl: string) {
    this.api = new ProductInventoryAPI({ baseUrl });
  }

  async initialize() {
    // Login with credentials
    const loginResult = await this.api.auth.login('manager@example.com', 'password123');
    if (!loginResult.success) {
      throw new Error('Authentication failed');
    }
  }

  async createProduct(productData: CreateProductRequest) {
    try {
      const result = await this.api.products.create(productData);
      
      if (result.success) {
        console.log('Product created:', result.data);
        return result.data;
      } else {
        console.error('Failed to create product:', result.message);
        return null;
      }
    } catch (error) {
      console.error('Error creating product:', error);
      throw error;
    }
  }

  async getProducts(filters?: {
    categoryId?: number;
    minPrice?: number;
    maxPrice?: number;
    inStock?: boolean;
    search?: string;
    page?: number;
    pageSize?: number;
  }) {
    try {
      const result = await this.api.products.getAll(filters);
      
      if (result.success) {
        return {
          products: result.data,
          pagination: result.pagination
        };
      } else {
        console.error('Failed to fetch products:', result.message);
        return null;
      }
    } catch (error) {
      console.error('Error fetching products:', error);
      throw error;
    }
  }

  async updateProductStock(productId: number, newStock: number, reason: string) {
    try {
      // Record inventory movement
      const movementResult = await this.api.inventoryMovement.record({
        productId,
        movementType: 'Adjustment',
        quantity: newStock,
        notes: reason,
        reference: `ADJ-${Date.now()}`
      });

      if (movementResult.success) {
        console.log('Stock updated successfully');
        return movementResult.data;
      } else {
        console.error('Failed to update stock:', movementResult.message);
        return null;
      }
    } catch (error) {
      console.error('Error updating stock:', error);
      throw error;
    }
  }

  async getProductAnalytics() {
    try {
      const [categoriesResult, summaryResult, alertsResult] = await Promise.all([
        this.api.analytics.getCategoryStatistics(),
        this.api.inventory.getSummary(),
        this.api.inventory.getStockAlerts({ level: 'low' })
      ]);

      return {
        categories: categoriesResult.success ? categoriesResult.data : [],
        summary: summaryResult.success ? summaryResult.data : null,
        alerts: alertsResult.success ? alertsResult.data : []
      };
    } catch (error) {
      console.error('Error fetching analytics:', error);
      throw error;
    }
  }
}

// Usage example
async function main() {
  const productManager = new ProductManager('https://api.productinventory.com/api/v1');
  
  try {
    await productManager.initialize();

    // Create a new product
    const newProduct = await productManager.createProduct({
      name: 'MacBook Pro 16"',
      description: 'High-performance laptop for professionals',
      sku: 'MBP-16-001',
      price: 2499.99,
      categoryId: 1,
      inventory: {
        currentStock: 50,
        reorderPoint: 10,
        maxStockLevel: 100
      }
    });

    if (newProduct) {
      console.log('Created product:', newProduct);

      // Get all products in the same category
      const products = await productManager.getProducts({
        categoryId: newProduct.categoryId,
        page: 1,
        pageSize: 20
      });

      console.log(`Found ${products?.products.length} products in category`);

      // Update stock level
      await productManager.updateProductStock(
        newProduct.id,
        -5, // Sold 5 units
        'Online sales order #12345'
      );

      // Get analytics
      const analytics = await productManager.getProductAnalytics();
      console.log('Analytics:', analytics);
    }
  } catch (error) {
    console.error('Application error:', error);
  }
}

main();
```

### Real-time Inventory Monitoring

```typescript
import { ProductInventoryAPI } from 'productinventory-api';

class InventoryMonitor {
  private api: ProductInventoryAPI;
  private monitoringInterval: NodeJS.Timeout | null = null;

  constructor(baseUrl: string) {
    this.api = new ProductInventoryAPI({ baseUrl });
  }

  async startMonitoring(intervalMs: number = 60000) {
    await this.api.auth.login('monitor@example.com', 'password123');

    this.monitoringInterval = setInterval(async () => {
      await this.checkInventoryLevels();
    }, intervalMs);

    console.log('Inventory monitoring started');
  }

  stopMonitoring() {
    if (this.monitoringInterval) {
      clearInterval(this.monitoringInterval);
      this.monitoringInterval = null;
      console.log('Inventory monitoring stopped');
    }
  }

  private async checkInventoryLevels() {
    try {
      // Get all stock alerts
      const alertsResult = await this.api.inventory.getStockAlerts();
      
      if (alertsResult.success && alertsResult.data.length > 0) {
        console.log(`Found ${alertsResult.data.length} stock alerts:`);
        
        for (const alert of alertsResult.data) {
          console.log(`- ${alert.productName} (${alert.sku}): ${alert.alertLevel.toUpperCase()}`);
          console.log(`  Current: ${alert.currentStock}, Reorder: ${alert.reorderPoint}`);
          
          if (alert.alertLevel === 'out') {
            await this.handleOutOfStock(alert);
          } else if (alert.alertLevel === 'low') {
            await this.handleLowStock(alert);
          }
        }
      }

      // Get inventory summary
      const summaryResult = await this.api.inventory.getSummary();
      if (summaryResult.success) {
        const summary = summaryResult.data;
        console.log(`Inventory Summary: ${summary.totalProducts} products, $${summary.totalValue.toFixed(2)} total value`);
        console.log(`Alerts: ${summary.lowStockItems} low stock, ${summary.outOfStockItems} out of stock`);
      }
    } catch (error) {
      console.error('Error during inventory monitoring:', error);
    }
  }

  private async handleOutOfStock(alert: any) {
    console.log(`🚨 OUT OF STOCK ALERT: ${alert.productName}`);
    // Implement notification logic (email, Slack, SMS, etc.)
    await this.sendAlert({
      level: 'critical',
      message: `Product ${alert.productName} (${alert.sku}) is out of stock!`,
      productId: alert.productId
    });
  }

  private async handleLowStock(alert: any) {
    console.log(`⚠️ LOW STOCK WARNING: ${alert.productName}`);
    // Implement notification logic
    await this.sendAlert({
      level: 'warning',
      message: `Product ${alert.productName} (${alert.sku}) is running low (${alert.currentStock} remaining)`,
      productId: alert.productId
    });
  }

  private async sendAlert(alertData: {
    level: 'critical' | 'warning';
    message: string;
    productId: number;
  }) {
    // Implement your notification system here
    // Examples: email, Slack webhook, SMS, push notification, etc.
    console.log(`[${alertData.level.toUpperCase()}] ${alertData.message}`);
  }
}

// Usage
const monitor = new InventoryMonitor('https://api.productinventory.com/api/v1');
monitor.startMonitoring(30000); // Check every 30 seconds

// Stop monitoring after 5 minutes (for demo)
setTimeout(() => {
  monitor.stopMonitoring();
}, 300000);
```

### E-commerce Integration

```typescript
import { ProductInventoryAPI } from 'productinventory-api';

class EcommerceIntegration {
  private api: ProductInventoryAPI;

  constructor(baseUrl: string) {
    this.api = new ProductInventoryAPI({ baseUrl });
  }

  async initialize() {
    await this.api.auth.login('ecommerce@example.com', 'password123');
  }

  async processOrder(orderData: {
    orderId: string;
    customerId: string;
    items: Array<{
      productId: number;
      quantity: number;
      price: number;
    }>;
  }) {
    try {
      // Validate stock availability
      const stockValidation = await this.validateStock(orderData.items);
      if (!stockValidation.valid) {
        return {
          success: false,
          message: 'Insufficient stock',
          errors: stockValidation.errors
        };
      }

      // Process inventory movements for each item
      const movements = [];
      for (const item of orderData.items) {
        const movementResult = await this.api.inventoryMovement.record({
          productId: item.productId,
          movementType: 'Sale',
          quantity: -item.quantity, // Negative for outgoing stock
          notes: `E-commerce order for customer ${orderData.customerId}`,
          reference: orderData.orderId
        });

        if (movementResult.success) {
          movements.push(movementResult.data);
        } else {
          // Rollback previous movements if one fails
          await this.rollbackMovements(movements);
          return {
            success: false,
            message: `Failed to process inventory for product ${item.productId}`,
            error: movementResult.message
          };
        }
      }

      return {
        success: true,
        message: 'Order processed successfully',
        movements
      };
    } catch (error) {
      console.error('Error processing order:', error);
      return {
        success: false,
        message: 'Internal error processing order',
        error: error.message
      };
    }
  }

  private async validateStock(items: Array<{ productId: number; quantity: number }>) {
    const errors = [];
    
    for (const item of items) {
      try {
        const productResult = await this.api.products.getById(item.productId);
        
        if (!productResult.success) {
          errors.push(`Product ${item.productId} not found`);
          continue;
        }

        const product = productResult.data;
        if (!product.inventory.isInStock || product.inventory.currentStock < item.quantity) {
          errors.push(`Insufficient stock for ${product.name}. Available: ${product.inventory.currentStock}, Required: ${item.quantity}`);
        }
      } catch (error) {
        errors.push(`Error validating stock for product ${item.productId}: ${error.message}`);
      }
    }

    return {
      valid: errors.length === 0,
      errors
    };
  }

  private async rollbackMovements(movements: any[]) {
    for (const movement of movements) {
      try {
        // Create opposite movement to rollback
        await this.api.inventoryMovement.record({
          productId: movement.productId,
          movementType: 'Adjustment',
          quantity: -movement.quantity, // Opposite quantity
          notes: `Rollback for failed order processing`,
          reference: `ROLLBACK-${movement.reference}`
        });
      } catch (error) {
        console.error('Error during rollback:', error);
      }
    }
  }

  async syncProductCatalog(externalProducts: Array<{
    externalId: string;
    name: string;
    description: string;
    price: number;
    categoryName: string;
  }>) {
    const results = [];

    for (const extProduct of externalProducts) {
      try {
        // Find or create category
        const categoriesResult = await this.api.categories.getAll({ search: extProduct.categoryName });
        let categoryId: number;

        if (categoriesResult.success && categoriesResult.data.length > 0) {
          categoryId = categoriesResult.data[0].id;
        } else {
          // Create new category
          const newCategoryResult = await this.api.categories.create({
            name: extProduct.categoryName,
            description: `Auto-created category for ${extProduct.categoryName}`
          });

          if (newCategoryResult.success) {
            categoryId = newCategoryResult.data.id;
          } else {
            results.push({
              externalId: extProduct.externalId,
              success: false,
              error: 'Failed to create category'
            });
            continue;
          }
        }

        // Create or update product
        const productResult = await this.api.products.create({
          name: extProduct.name,
          description: extProduct.description,
          sku: `EXT-${extProduct.externalId}`,
          price: extProduct.price,
          categoryId,
          inventory: {
            currentStock: 0, // Will be updated separately
            reorderPoint: 10,
            maxStockLevel: 100
          }
        });

        results.push({
          externalId: extProduct.externalId,
          success: productResult.success,
          productId: productResult.success ? productResult.data.id : null,
          error: productResult.success ? null : productResult.message
        });
      } catch (error) {
        results.push({
          externalId: extProduct.externalId,
          success: false,
          error: error.message
        });
      }
    }

    return results;
  }
}

// Usage example
async function ecommerceExample() {
  const integration = new EcommerceIntegration('https://api.productinventory.com/api/v1');
  await integration.initialize();

  // Process an order
  const orderResult = await integration.processOrder({
    orderId: 'ORDER-2024-001',
    customerId: 'CUST-12345',
    items: [
      { productId: 1, quantity: 2, price: 99.99 },
      { productId: 2, quantity: 1, price: 149.99 }
    ]
  });

  console.log('Order processing result:', orderResult);

  // Sync external product catalog
  const syncResult = await integration.syncProductCatalog([
    {
      externalId: 'EXT-001',
      name: 'External Product 1',
      description: 'Product from external system',
      price: 79.99,
      categoryName: 'External Category'
    }
  ]);

  console.log('Catalog sync result:', syncResult);
}
```

### Batch Operations

```typescript
import { ProductInventoryAPI } from 'productinventory-api';

class BatchOperations {
  private api: ProductInventoryAPI;
  private batchSize: number = 10;

  constructor(baseUrl: string, batchSize: number = 10) {
    this.api = new ProductInventoryAPI({ baseUrl });
    this.batchSize = batchSize;
  }

  async initialize() {
    await this.api.auth.login('batch@example.com', 'password123');
  }

  async batchUpdatePrices(priceUpdates: Array<{ productId: number; newPrice: number }>) {
    const results = [];
    
    // Process in batches to avoid overwhelming the API
    for (let i = 0; i < priceUpdates.length; i += this.batchSize) {
      const batch = priceUpdates.slice(i, i + this.batchSize);
      const batchResults = await Promise.allSettled(
        batch.map(async (update) => {
          try {
            // Get current product data
            const productResult = await this.api.products.getById(update.productId);
            if (!productResult.success) {
              throw new Error(`Product ${update.productId} not found`);
            }

            // Update with new price
            const updateResult = await this.api.products.update(update.productId, {
              ...productResult.data,
              price: update.newPrice
            });

            return {
              productId: update.productId,
              success: updateResult.success,
              oldPrice: productResult.data.price,
              newPrice: update.newPrice,
              error: updateResult.success ? null : updateResult.message
            };
          } catch (error) {
            return {
              productId: update.productId,
              success: false,
              error: error.message
            };
          }
        })
      );

      // Collect results
      batchResults.forEach((result, index) => {
        if (result.status === 'fulfilled') {
          results.push(result.value);
        } else {
          results.push({
            productId: batch[index].productId,
            success: false,
            error: result.reason.message
          });
        }
      });

      // Add delay between batches to be API-friendly
      if (i + this.batchSize < priceUpdates.length) {
        await new Promise(resolve => setTimeout(resolve, 1000));
      }
    }

    return results;
  }

  async exportInventoryReport(format: 'csv' | 'json' = 'json') {
    try {
      // Get all products with inventory data
      const allProducts = [];
      let page = 1;
      let hasMore = true;

      while (hasMore) {
        const result = await this.api.products.getAll({
          page,
          pageSize: 100,
          sortBy: 'name',
          sortDirection: 'asc'
        });

        if (result.success && result.data.length > 0) {
          allProducts.push(...result.data);
          hasMore = result.pagination.hasNextPage;
          page++;
        } else {
          hasMore = false;
        }
      }

      // Get analytics data
      const analyticsResult = await this.api.analytics.getCategoryStatistics();
      const analytics = analyticsResult.success ? analyticsResult.data : [];

      // Combine data
      const reportData = allProducts.map(product => ({
        productId: product.id,
        name: product.name,
        sku: product.sku,
        category: product.category?.name || 'Unknown',
        price: product.price,
        currentStock: product.inventory?.currentStock || 0,
        reorderPoint: product.inventory?.reorderPoint || 0,
        maxStockLevel: product.inventory?.maxStockLevel || 0,
        stockValue: product.price * (product.inventory?.currentStock || 0),
        isInStock: product.inventory?.isInStock || false,
        needsReorder: (product.inventory?.currentStock || 0) <= (product.inventory?.reorderPoint || 0),
        lastUpdated: product.updatedAt
      }));

      if (format === 'csv') {
        return this.convertToCSV(reportData);
      } else {
        return {
          timestamp: new Date().toISOString(),
          totalProducts: reportData.length,
          summary: {
            totalValue: reportData.reduce((sum, item) => sum + item.stockValue, 0),
            inStock: reportData.filter(item => item.isInStock).length,
            needsReorder: reportData.filter(item => item.needsReorder).length
          },
          products: reportData,
          categoryAnalytics: analytics
        };
      }
    } catch (error) {
      console.error('Error generating inventory report:', error);
      throw error;
    }
  }

  private convertToCSV(data: any[]): string {
    if (data.length === 0) return '';

    const headers = Object.keys(data[0]);
    const csvHeaders = headers.join(',');
    
    const csvRows = data.map(row => {
      return headers.map(header => {
        const value = row[header];
        // Escape commas and quotes
        if (typeof value === 'string' && (value.includes(',') || value.includes('"'))) {
          return `"${value.replace(/"/g, '""')}"`;
        }
        return value;
      }).join(',');
    });

    return [csvHeaders, ...csvRows].join('\n');
  }
}

// Usage example
async function batchExample() {
  const batchOps = new BatchOperations('https://api.productinventory.com/api/v1');
  await batchOps.initialize();

  // Batch price updates (e.g., applying 10% increase)
  const priceUpdates = [
    { productId: 1, newPrice: 109.99 },
    { productId: 2, newPrice: 164.99 },
    { productId: 3, newPrice: 54.99 }
  ];

  const updateResults = await batchOps.batchUpdatePrices(priceUpdates);
  console.log('Price update results:', updateResults);

  // Generate inventory report
  const report = await batchOps.exportInventoryReport('json');
  console.log('Inventory report generated:', report.summary);

  // Generate CSV report
  const csvReport = await batchOps.exportInventoryReport('csv');
  console.log('CSV report length:', csvReport.length);
}
```

## Error Handling

```typescript
import { ProductInventoryAPI, ApiError } from 'productinventory-api';

const api = new ProductInventoryAPI({ baseUrl: 'https://api.productinventory.com/api/v1' });

try {
  await api.auth.login('user@example.com', 'wrongpassword');
} catch (error) {
  if (error instanceof ApiError) {
    console.log('API Error:', error.message);
    console.log('Status Code:', error.statusCode);
    console.log('Error Code:', error.errorCode);
    console.log('Correlation ID:', error.correlationId);
    
    if (error.validationErrors) {
      console.log('Validation Errors:', error.validationErrors);
    }
  } else {
    console.log('Network Error:', error.message);
  }
}
```

## Configuration Options

```typescript
const api = new ProductInventoryAPI({
  baseUrl: 'https://api.productinventory.com/api/v1',
  timeout: 30000, // Request timeout in milliseconds
  retries: 3, // Number of retries for failed requests
  retryDelay: 1000, // Delay between retries
  apiKey: 'your-api-key', // Optional API key
  userAgent: 'MyApp/1.0.0', // Custom user agent
  defaultHeaders: {
    'X-Client-Version': '1.0.0'
  }
});
```

## TypeScript Types

The SDK includes comprehensive TypeScript definitions:

```typescript
import {
  Product,
  Category,
  InventoryMovement,
  CategoryStatistics,
  CreateProductRequest,
  UpdateProductRequest,
  PaginatedResponse,
  ApiResponse
} from 'productinventory-api';

// All API responses are properly typed
const products: ApiResponse<Product[]> = await api.products.getAll();
const product: ApiResponse<Product> = await api.products.getById(1);
const stats: ApiResponse<CategoryStatistics[]> = await api.analytics.getCategoryStatistics();
```