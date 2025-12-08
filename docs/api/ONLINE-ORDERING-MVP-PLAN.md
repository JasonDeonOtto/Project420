# Online Ordering API MVP - Implementation Plan

**Project**: Project420 - Cannabis Management System
**Feature**: Online Ordering API (Click & Collect)
**Status**: 🚀 **READY TO BUILD**
**Compliance**: Cannabis for Private Purposes Act 2024 + POPIA
**Target Date**: Phase 5 (Future-proofing for 2026-2027 commercial regulations)

---

## 🎯 MVP Objectives

### Primary Goal
Build a compliant **"Click & Collect"** online ordering system that:
1. Allows customers to browse products online
2. Place orders with age verification
3. Pay online (Yoco/PayFast/Ozow)
4. Collect in-store with ID verification
5. Full POPIA compliance and audit trails

### Legal Compliance Strategy
⚠️ **CRITICAL**: Commercial cannabis sales NOT yet legal in SA (2024)
- Current implementation: **Click & Collect only** (order online, pickup in-store)
- Future-ready: Easy activation of delivery when regulations permit (2026-2027)
- Full age verification at both registration AND pickup
- Complete audit trail for regulatory readiness

---

## 📐 Architecture Overview

### System Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    Client Applications                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │  Web Browser │  │ Mobile App  │  │  POS System │          │
│  │   (Blazor)   │  │   (MAUI)    │  │  (Blazor)   │          │
│  └──────┬───────┘  └──────┬──────┘  └──────┬──────┘          │
└─────────┼──────────────────┼─────────────────┼────────────────┘
          │                  │                 │
          └──────────────────┼─────────────────┘
                             │
          ┌──────────────────▼──────────────────┐
          │     API Gateway / Load Balancer     │
          │         (Future: nginx/IIS)         │
          └──────────────────┬──────────────────┘
                             │
          ┌──────────────────▼──────────────────┐
          │   Project420.API.WebApi (.NET 9)    │
          │  ┌─────────────────────────────┐    │
          │  │    Compliance Middleware    │    │
          │  │ • Age Verification          │    │
          │  │ • Audit Logging (POPIA)     │    │
          │  │ • Rate Limiting             │    │
          │  │ • CORS                      │    │
          │  └─────────────────────────────┘    │
          │  ┌─────────────────────────────┐    │
          │  │      API Controllers        │    │
          │  │ • Products (Catalog)        │    │
          │  │ • Orders (Click & Collect)  │    │
          │  │ • Customers (Registration)  │    │
          │  │ • Auth (JWT/OAuth)          │    │
          │  │ • Payments (Integrations)   │    │
          │  └─────────────────────────────┘    │
          └──────────────────┬──────────────────┘
                             │
          ┌──────────────────▼──────────────────┐
          │      OnlineOrders Module (BLL)      │
          │  • Order Processing                 │
          │  • Payment Orchestration            │
          │  • Notification Service             │
          │  • Inventory Reservation            │
          └──────────────────┬──────────────────┘
                             │
          ┌──────────────────▼──────────────────┐
          │      OnlineOrders Module (DAL)      │
          │  • Order Repository                 │
          │  • Customer Repository              │
          │  • Payment Transaction Log          │
          └──────────────────┬──────────────────┘
                             │
          ┌──────────────────▼──────────────────┐
          │        Database (PostgreSQL)        │
          │  • online_orders                    │
          │  • online_order_items               │
          │  • customer_accounts                │
          │  • payment_transactions             │
          │  • pickup_confirmations             │
          └─────────────────────────────────────┘
                             │
          ┌──────────────────▼──────────────────┐
          │      External Integrations          │
          │  ┌──────────┐  ┌──────────┐         │
          │  │  Yoco    │  │ PayFast  │         │
          │  │ (Card)   │  │ (Card)   │         │
          │  └──────────┘  └──────────┘         │
          │  ┌──────────┐  ┌──────────┐         │
          │  │  Ozow    │  │  Email   │         │
          │  │ (EFT)    │  │ (SMTP)   │         │
          │  └──────────┘  └──────────┘         │
          └─────────────────────────────────────┘
```

---

## 📁 Project Structure

```
Project420/
├── src/
│   ├── API/
│   │   └── Project420.API.WebApi/              ← NEW: Web API Project
│   │       ├── Controllers/
│   │       │   ├── ProductsController.cs       ← Product catalog endpoints
│   │       │   ├── OrdersController.cs         ← Order management
│   │       │   ├── CustomersController.cs      ← Customer accounts
│   │       │   ├── AuthController.cs           ← Authentication/JWT
│   │       │   └── PaymentsController.cs       ← Payment webhooks
│   │       ├── Middleware/
│   │       │   ├── AgeVerificationMiddleware.cs
│   │       │   ├── AuditLoggingMiddleware.cs
│   │       │   ├── RateLimitingMiddleware.cs
│   │       │   └── ExceptionHandlingMiddleware.cs
│   │       ├── Configuration/
│   │       │   ├── JwtSettings.cs
│   │       │   ├── PaymentSettings.cs
│   │       │   └── EmailSettings.cs
│   │       ├── Extensions/
│   │       │   ├── ServiceCollectionExtensions.cs
│   │       │   └── ApplicationBuilderExtensions.cs
│   │       ├── Filters/
│   │       │   ├── ValidateModelAttribute.cs
│   │       │   └── RequireAgeVerificationAttribute.cs
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── Project420.API.WebApi.csproj
│   │
│   ├── Modules/
│   │   ├── OnlineOrders/                       ← NEW: Online Orders Module
│   │   │   ├── Project420.OnlineOrders.Models/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── OnlineOrder.cs
│   │   │   │   │   ├── OnlineOrderItem.cs
│   │   │   │   │   ├── CustomerAccount.cs
│   │   │   │   │   ├── PaymentTransaction.cs
│   │   │   │   │   ├── PickupConfirmation.cs
│   │   │   │   │   └── OrderStatusHistory.cs
│   │   │   │   └── Enums/
│   │   │   │       ├── OnlineOrderStatus.cs
│   │   │   │       ├── PaymentProvider.cs
│   │   │   │       └── PickupVerificationMethod.cs
│   │   │   │
│   │   │   ├── Project420.OnlineOrders.DAL/
│   │   │   │   ├── OnlineOrdersDbContext.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── IOnlineOrderRepository.cs
│   │   │   │   │   ├── OnlineOrderRepository.cs
│   │   │   │   │   ├── ICustomerAccountRepository.cs
│   │   │   │   │   └── CustomerAccountRepository.cs
│   │   │   │   └── Migrations/
│   │   │   │
│   │   │   └── Project420.OnlineOrders.BLL/
│   │   │       ├── DTOs/
│   │   │       │   ├── Request/
│   │   │       │   │   ├── CreateOrderRequestDto.cs
│   │   │       │   │   ├── CustomerRegistrationDto.cs
│   │   │       │   │   ├── PaymentRequestDto.cs
│   │   │       │   │   └── PickupConfirmationDto.cs
│   │   │       │   └── Response/
│   │   │       │       ├── OrderResponseDto.cs
│   │   │       │       ├── OrderStatusDto.cs
│   │   │       │       ├── PaymentResponseDto.cs
│   │   │       │       └── ProductCatalogDto.cs
│   │   │       ├── Services/
│   │   │       │   ├── IOnlineOrderService.cs
│   │   │       │   ├── OnlineOrderService.cs
│   │   │       │   ├── IPaymentOrchestrationService.cs
│   │   │       │   ├── PaymentOrchestrationService.cs
│   │   │       │   ├── IInventoryReservationService.cs
│   │   │       │   ├── InventoryReservationService.cs
│   │   │       │   ├── INotificationService.cs
│   │   │       │   └── NotificationService.cs
│   │   │       ├── Validators/
│   │   │       │   ├── CreateOrderValidator.cs
│   │   │       │   ├── CustomerRegistrationValidator.cs
│   │   │       │   └── PickupConfirmationValidator.cs
│   │   │       └── Integrations/
│   │   │           ├── Yoco/
│   │   │           │   ├── IYocoPaymentClient.cs
│   │   │           │   └── YocoPaymentClient.cs
│   │   │           ├── PayFast/
│   │   │           │   ├── IPayFastPaymentClient.cs
│   │   │           │   └── PayFastPaymentClient.cs
│   │   │           └── Ozow/
│   │   │               ├── IOzowPaymentClient.cs
│   │   │               └── OzowPaymentClient.cs
│   │   │
│   │   ├── Management/                         ← EXISTING
│   │   └── Retail/                             ← EXISTING
│   │
│   └── Shared/                                 ← EXISTING
│
├── tests/
│   └── Project420.OnlineOrders.Tests/          ← NEW: API Tests
│       ├── Controllers/
│       ├── Services/
│       └── Integration/
│
└── docs/
    └── api/
        ├── ONLINE-ORDERING-MVP-PLAN.md         ← THIS FILE
        ├── API-ENDPOINTS.md                    ← API documentation
        ├── PAYMENT-INTEGRATION.md              ← Payment provider docs
        └── DEPLOYMENT-GUIDE.md                 ← Deployment instructions
```

---

## 🔌 API Endpoints Specification

### 1. Product Catalog API

**GET /api/products**
- List all available products (paginated)
- Filter by category, strain, THC/CBD content
- Search by name/description
- Sort by price, popularity, name

**GET /api/products/{id}**
- Get detailed product information
- Includes: description, pricing, stock availability, THC/CBD content, lab results

**GET /api/products/categories**
- List all product categories

**Response Example**:
```json
{
  "products": [
    {
      "id": 123,
      "name": "Blue Dream - Premium Flower",
      "categoryCode": "FLOWER",
      "price": 150.00,
      "priceInclVAT": 172.50,
      "thcContent": 18.5,
      "cbdContent": 0.8,
      "strainType": "Hybrid",
      "stockAvailable": true,
      "stockQuantity": 25,
      "imageUrl": "/images/products/blue-dream.jpg",
      "labTestDate": "2025-12-01",
      "batchNumber": "BD-20251201-001"
    }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20
}
```

---

### 2. Customer Authentication API

**POST /api/auth/register**
- Create new customer account
- **CRITICAL**: Age verification (18+)
- POPIA consent required
- Email verification

**POST /api/auth/login**
- Customer login
- Returns JWT token
- Refresh token for long sessions

**POST /api/auth/refresh**
- Refresh JWT token

**POST /api/auth/verify-age**
- Age verification with ID document
- Required before first order

**Request Example**:
```json
{
  "email": "customer@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "idNumber": "9501015800081",
  "phoneNumber": "+27821234567",
  "dateOfBirth": "1995-01-01",
  "consentToPOPIA": true,
  "consentToMarketing": false
}
```

**Response Example**:
```json
{
  "success": true,
  "customerId": 456,
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "RT_abc123...",
  "expiresIn": 3600,
  "ageVerified": true,
  "requiresIDVerification": false
}
```

---

### 3. Online Orders API

**POST /api/orders**
- Create new online order (Click & Collect)
- Reserve inventory
- Calculate totals (VAT, discounts)
- Return payment link

**GET /api/orders/{id}**
- Get order details and status

**GET /api/orders/my-orders**
- List customer's orders (authenticated)

**PATCH /api/orders/{id}/status**
- Update order status (admin only)

**POST /api/orders/{id}/confirm-pickup**
- Confirm in-store pickup with ID verification
- **CRITICAL**: Age verification at pickup

**Request Example**:
```json
{
  "customerId": 456,
  "items": [
    {
      "productId": 123,
      "quantity": 2,
      "priceAtTimeOfOrder": 150.00
    }
  ],
  "pickupLocationId": 1,
  "preferredPickupDate": "2025-12-08",
  "preferredPickupTime": "14:00",
  "notes": "Please call when ready"
}
```

**Response Example**:
```json
{
  "success": true,
  "orderId": 789,
  "orderNumber": "ORD-20251207-001",
  "status": "PendingPayment",
  "subtotal": 300.00,
  "vatAmount": 45.00,
  "totalAmount": 345.00,
  "paymentUrl": "https://pay.yoco.com/abc123",
  "expiresAt": "2025-12-07T14:30:00Z",
  "pickupLocation": {
    "name": "Main Store",
    "address": "123 Main Street, Cape Town",
    "phone": "+27215551234"
  }
}
```

---

### 4. Payment Integration API

**POST /api/payments/initiate**
- Initiate payment with provider (Yoco/PayFast/Ozow)
- Returns payment URL for redirect

**POST /api/payments/webhook/yoco**
- Yoco payment webhook (success/failure)

**POST /api/payments/webhook/payfast**
- PayFast IPN (Instant Payment Notification)

**POST /api/payments/webhook/ozow**
- Ozow payment notification

**GET /api/payments/{orderId}/status**
- Check payment status

---

### 5. Order Status Tracking API

**GET /api/orders/{id}/status**
- Get current order status

**GET /api/orders/{id}/history**
- Get full order status history (audit trail)

**Order Status Flow**:
```
Draft → PendingPayment → PaymentReceived → ReadyForPickup →
Completed → [Optional: Cancelled/Refunded]
```

---

## 🗄️ Database Schema

### OnlineOrders Database Tables

```sql
-- Online Orders
CREATE TABLE online_orders (
    id SERIAL PRIMARY KEY,
    order_number VARCHAR(50) UNIQUE NOT NULL,
    customer_id INTEGER NOT NULL,
    order_date TIMESTAMP NOT NULL DEFAULT NOW(),
    status VARCHAR(50) NOT NULL,

    -- Order Totals
    subtotal DECIMAL(18,2) NOT NULL,
    vat_amount DECIMAL(18,2) NOT NULL,
    discount_amount DECIMAL(18,2) DEFAULT 0,
    total_amount DECIMAL(18,2) NOT NULL,

    -- Pickup Information
    pickup_location_id INTEGER NOT NULL,
    preferred_pickup_date DATE,
    preferred_pickup_time TIME,
    actual_pickup_date TIMESTAMP,

    -- Customer Notes
    customer_notes TEXT,
    internal_notes TEXT,

    -- Payment Information
    payment_provider VARCHAR(50),
    payment_reference VARCHAR(100),
    payment_date TIMESTAMP,

    -- Compliance (POPIA + Cannabis Act)
    age_verified_at_order BOOLEAN DEFAULT FALSE,
    age_verified_at_pickup BOOLEAN DEFAULT FALSE,
    pickup_verified_by INTEGER,
    id_verification_method VARCHAR(50),

    -- Audit Fields (POPIA requirement)
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_at TIMESTAMP,
    updated_by VARCHAR(100),

    CONSTRAINT fk_customer FOREIGN KEY (customer_id)
        REFERENCES customer_accounts(id),
    CONSTRAINT fk_pickup_location FOREIGN KEY (pickup_location_id)
        REFERENCES pickup_locations(id)
);

-- Online Order Items
CREATE TABLE online_order_items (
    id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,

    -- Product Details (snapshot at time of order)
    product_name VARCHAR(200) NOT NULL,
    product_code VARCHAR(50),
    category_code VARCHAR(50),

    -- Pricing (snapshot at time of order)
    quantity INTEGER NOT NULL,
    unit_price DECIMAL(18,2) NOT NULL,
    line_subtotal DECIMAL(18,2) NOT NULL,
    line_vat DECIMAL(18,2) NOT NULL,
    line_total DECIMAL(18,2) NOT NULL,

    -- Cannabis Compliance (snapshot at time of order)
    thc_content DECIMAL(5,2),
    cbd_content DECIMAL(5,2),
    strain_type VARCHAR(50),
    batch_number VARCHAR(100),
    lab_test_date DATE,

    -- Fulfillment
    reserved BOOLEAN DEFAULT FALSE,
    reserved_at TIMESTAMP,
    picked BOOLEAN DEFAULT FALSE,
    picked_at TIMESTAMP,

    CONSTRAINT fk_order FOREIGN KEY (order_id)
        REFERENCES online_orders(id) ON DELETE CASCADE,
    CONSTRAINT fk_product FOREIGN KEY (product_id)
        REFERENCES products(id)
);

-- Customer Accounts (Online Customers)
CREATE TABLE customer_accounts (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(500) NOT NULL,

    -- Personal Information (POPIA protected)
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    id_number VARCHAR(20) UNIQUE,
    date_of_birth DATE NOT NULL,
    phone_number VARCHAR(20),

    -- Age Verification (Cannabis Act requirement)
    age_verified BOOLEAN DEFAULT FALSE,
    age_verification_date TIMESTAMP,
    age_verification_method VARCHAR(50),
    id_document_verified BOOLEAN DEFAULT FALSE,

    -- POPIA Compliance
    consent_to_popia BOOLEAN NOT NULL,
    consent_to_popia_date TIMESTAMP NOT NULL,
    consent_to_marketing BOOLEAN DEFAULT FALSE,
    consent_to_marketing_date TIMESTAMP,

    -- Account Status
    is_active BOOLEAN DEFAULT TRUE,
    is_locked BOOLEAN DEFAULT FALSE,
    email_verified BOOLEAN DEFAULT FALSE,
    email_verification_token VARCHAR(500),
    email_verified_at TIMESTAMP,

    -- Password Reset
    password_reset_token VARCHAR(500),
    password_reset_expires TIMESTAMP,

    -- Audit Fields
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    last_login_at TIMESTAMP,

    CONSTRAINT chk_age CHECK (EXTRACT(YEAR FROM AGE(date_of_birth)) >= 18)
);

-- Payment Transactions (Audit Trail)
CREATE TABLE payment_transactions (
    id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL,

    -- Payment Provider
    provider VARCHAR(50) NOT NULL, -- Yoco, PayFast, Ozow
    provider_transaction_id VARCHAR(200),
    provider_reference VARCHAR(200),

    -- Transaction Details
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'ZAR',
    status VARCHAR(50) NOT NULL, -- Pending, Success, Failed, Refunded

    -- Payment Method
    payment_method VARCHAR(50), -- Card, EFT, Instant EFT
    card_last_four VARCHAR(4),
    card_type VARCHAR(50),

    -- Timestamps
    initiated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMP,
    failed_at TIMESTAMP,

    -- Error Information
    error_code VARCHAR(50),
    error_message TEXT,

    -- Webhook Data (raw JSON for audit)
    webhook_payload JSONB,

    -- SARS Compliance
    receipt_number VARCHAR(100),
    receipt_generated_at TIMESTAMP,

    CONSTRAINT fk_order FOREIGN KEY (order_id)
        REFERENCES online_orders(id)
);

-- Pickup Confirmations (Age Verification Audit Trail)
CREATE TABLE pickup_confirmations (
    id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL UNIQUE,

    -- Pickup Details
    pickup_date TIMESTAMP NOT NULL,
    picked_up_by_customer_id INTEGER NOT NULL,

    -- Age Verification (Cannabis Act CRITICAL)
    id_verification_method VARCHAR(50) NOT NULL, -- IDDocument, Passport, DriversLicense
    id_number_verified VARCHAR(20),
    age_confirmed BOOLEAN NOT NULL,
    verified_by_staff_id INTEGER NOT NULL,

    -- Digital Signature/Photo (Optional)
    customer_signature BYTEA,
    id_photo BYTEA,

    -- Notes
    verification_notes TEXT,

    -- Audit Fields
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),

    CONSTRAINT fk_order FOREIGN KEY (order_id)
        REFERENCES online_orders(id),
    CONSTRAINT fk_customer FOREIGN KEY (picked_up_by_customer_id)
        REFERENCES customer_accounts(id)
);

-- Order Status History (Audit Trail for POPIA)
CREATE TABLE order_status_history (
    id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL,

    -- Status Change
    old_status VARCHAR(50),
    new_status VARCHAR(50) NOT NULL,

    -- Reason/Notes
    change_reason TEXT,

    -- Audit Fields
    changed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    changed_by VARCHAR(100) NOT NULL,

    CONSTRAINT fk_order FOREIGN KEY (order_id)
        REFERENCES online_orders(id) ON DELETE CASCADE
);

-- Pickup Locations
CREATE TABLE pickup_locations (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    address_line1 VARCHAR(200) NOT NULL,
    address_line2 VARCHAR(200),
    city VARCHAR(100) NOT NULL,
    province VARCHAR(50) NOT NULL,
    postal_code VARCHAR(10) NOT NULL,
    phone_number VARCHAR(20),
    email VARCHAR(255),

    -- Operating Hours
    opening_hours JSONB, -- {"monday": "09:00-17:00", ...}

    -- Status
    is_active BOOLEAN DEFAULT TRUE,

    -- GPS Coordinates (optional)
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8)
);

-- Indexes for Performance
CREATE INDEX idx_online_orders_customer ON online_orders(customer_id);
CREATE INDEX idx_online_orders_status ON online_orders(status);
CREATE INDEX idx_online_orders_date ON online_orders(order_date);
CREATE INDEX idx_online_order_items_order ON online_order_items(order_id);
CREATE INDEX idx_customer_accounts_email ON customer_accounts(email);
CREATE INDEX idx_payment_transactions_order ON payment_transactions(order_id);
CREATE INDEX idx_payment_transactions_provider ON payment_transactions(provider, provider_transaction_id);
```

---

## 🔐 Security & Compliance

### Authentication & Authorization

**JWT Token Structure**:
```json
{
  "sub": "456",  // Customer ID
  "email": "customer@example.com",
  "name": "John Doe",
  "role": "Customer",
  "ageVerified": true,
  "iat": 1733580000,
  "exp": 1733583600,
  "iss": "Project420.API",
  "aud": "Project420.Clients"
}
```

**Required Claims**:
- `ageVerified`: Must be `true` to place orders (Cannabis Act)
- `role`: Customer, Staff, Admin
- `exp`: Token expiration (1 hour default)

---

### POPIA Compliance Checklist

- [x] **Consent Tracking**: Customer must consent at registration
- [x] **Data Encryption**: PII encrypted at rest (database) and in transit (HTTPS)
- [x] **Access Control**: Role-based access (RBAC)
- [x] **Audit Logging**: All data access logged with timestamps
- [x] **Data Retention**: 7-year retention for financial/compliance data
- [x] **Right to be Forgotten**: Customer can request data deletion
- [x] **Data Portability**: Customer can export their data

---

### Cannabis Act Compliance Checklist

- [x] **Age Verification**: 18+ at registration AND pickup
- [x] **Purchase Limits**: Enforce possession limits per transaction
- [x] **Batch Tracking**: All products linked to batch numbers
- [x] **Lab Results**: THC/CBD content verified and displayed
- [x] **Audit Trail**: Complete order history for regulatory inspection
- [x] **ID Verification at Pickup**: Photo ID required, logged in database
- [x] **No Delivery**: Click & Collect only (until regulations permit)

---

## 💳 Payment Integration

### Supported Payment Providers (SA Market)

#### 1. **Yoco** (Recommended for Card Payments)
- **Best For**: Card payments (Visa, Mastercard)
- **Fees**: ~2.95% per transaction
- **Integration**: REST API + Webhooks
- **Settlement**: T+3 days
- **PCI Compliance**: Yoco is PCI-DSS Level 1 compliant

**Implementation**:
```csharp
public interface IYocoPaymentClient
{
    Task<YocoCheckoutResponse> CreateCheckoutAsync(YocoCheckoutRequest request);
    Task<YocoPaymentStatus> GetPaymentStatusAsync(string checkoutId);
    Task<bool> VerifyWebhookSignature(string payload, string signature);
}
```

#### 2. **PayFast** (Widely Used in SA)
- **Best For**: Card, EFT, Instant EFT, SnapScan, Zapper
- **Fees**: 2.9% + R2.00 per transaction
- **Integration**: Form POST + IPN (Instant Payment Notification)
- **Settlement**: T+2 days

#### 3. **Ozow** (Best for Instant EFT)
- **Best For**: Instant bank transfers (EFT)
- **Fees**: 1.5% - 2.5% per transaction
- **Integration**: REST API + Webhooks
- **Settlement**: Real-time to T+1 day

---

### Payment Flow

```
1. Customer adds products to cart
2. Customer proceeds to checkout
3. API creates order with status "PendingPayment"
4. API calls payment provider API to create payment link
5. Customer redirected to payment provider (Yoco/PayFast/Ozow)
6. Customer completes payment
7. Payment provider sends webhook to /api/payments/webhook/{provider}
8. API validates webhook signature
9. API updates order status to "PaymentReceived"
10. API reserves inventory
11. API sends email confirmation to customer
12. Order status updated to "ReadyForPickup"
13. Customer collects in-store with ID verification
14. Staff confirms pickup and age verification
15. Order status updated to "Completed"
```

---

## 📧 Notification System

### Email Templates Required

1. **Order Confirmation** (Sent after payment)
   - Order summary
   - Pickup location and hours
   - Reminder: Bring ID for age verification

2. **Ready for Pickup** (Sent when order ready)
   - Pickup location details
   - Operating hours
   - Age verification reminder

3. **Pickup Reminder** (Sent 24h before expiry)
   - Order will expire in 24 hours
   - Pickup details

4. **Order Expired** (Sent if not picked up)
   - Refund initiated
   - Customer service contact

5. **Refund Processed** (Sent after refund)
   - Refund amount
   - Processing time (5-10 business days)

---

## 📊 Monitoring & Analytics

### Key Metrics to Track

**Business Metrics**:
- Orders per day/week/month
- Average order value
- Conversion rate (views → orders)
- Pickup rate (orders → picked up)
- Cancellation rate
- Top-selling products

**Technical Metrics**:
- API response times (target: <500ms)
- Error rate (target: <1%)
- Payment success rate (target: >95%)
- Webhook delivery success rate

**Compliance Metrics**:
- Age verification success rate (target: 100%)
- Failed age verifications (investigate)
- Audit log completeness (target: 100%)
- POPIA consent rate (target: 100%)

---

## 🚀 Deployment Strategy

### Phase 1: Development (Current)
- Local development environment
- SQLite/PostgreSQL database
- Test payment providers (sandbox mode)
- Localhost API (https://localhost:7001)

### Phase 2: Staging
- Azure App Service / AWS / DigitalOcean
- PostgreSQL database (managed)
- Test payment providers (sandbox mode)
- Staging URL (https://api-staging.project420.co.za)

### Phase 3: Production
- Azure App Service / AWS / DigitalOcean (load balanced)
- PostgreSQL database (managed, replicated)
- Production payment providers
- Production URL (https://api.project420.co.za)
- SSL/TLS certificate (Let's Encrypt)
- CORS configured for web/mobile apps
- Rate limiting enabled
- DDoS protection

---

## 🧪 Testing Strategy

### Unit Tests
- Controller tests (200+ tests target)
- Service tests (business logic)
- Validator tests (FluentValidation)
- Payment integration tests (mocked)

### Integration Tests
- End-to-end order flow
- Payment provider integration (sandbox)
- Database integration
- Email delivery

### Security Tests
- Age verification bypass attempts
- JWT token validation
- CORS policy enforcement
- SQL injection protection
- XSS protection

### Load Tests
- 100 concurrent users
- 1000 orders per hour
- Database connection pooling
- API response time <500ms

---

## 📅 Implementation Timeline

### Week 1: Project Setup & Infrastructure
- [ ] Create API project (Project420.API.WebApi)
- [ ] Create OnlineOrders module (Models, DAL, BLL)
- [ ] Set up PostgreSQL database
- [ ] Configure JWT authentication
- [ ] Set up Swagger/OpenAPI documentation

### Week 2: Core API Development
- [ ] Products API (catalog endpoints)
- [ ] Customers API (registration, authentication)
- [ ] Orders API (create, read, update)
- [ ] Compliance middleware (age verification, audit logging)

### Week 3: Payment Integration
- [ ] Yoco integration (checkout, webhooks)
- [ ] PayFast integration (IPN)
- [ ] Ozow integration (instant EFT)
- [ ] Payment status tracking

### Week 4: Notifications & Workflow
- [ ] Email service (SMTP)
- [ ] Order confirmation emails
- [ ] Pickup notification emails
- [ ] Order status workflow

### Week 5: Testing & Refinement
- [ ] Unit tests (90%+ coverage)
- [ ] Integration tests
- [ ] Security testing
- [ ] Performance testing

### Week 6: Deployment & Documentation
- [ ] Deploy to staging
- [ ] API documentation (Swagger)
- [ ] User guide (customer-facing)
- [ ] Admin guide (staff-facing)

---

## ✅ Success Criteria

### Technical
- [x] All API endpoints functional and documented
- [x] 90%+ test coverage
- [x] <500ms average response time
- [x] >99% uptime (production)
- [x] Zero security vulnerabilities (OWASP Top 10)

### Business
- [x] Customers can browse products online
- [x] Customers can place orders and pay online
- [x] Staff can manage orders and confirm pickups
- [x] Email notifications working
- [x] Complete audit trail for compliance

### Compliance
- [x] 100% age verification at registration and pickup
- [x] POPIA consent tracked and enforced
- [x] Complete audit logs (7-year retention)
- [x] Cannabis Act compliance verified
- [x] SARS tax reporting ready

---

**Status**: 🚀 **READY TO BUILD**
**Next Step**: Create API project scaffolding

---

*Last Updated: 2025-12-07*
*Project: Project420 - Cannabis Management System for South Africa*
