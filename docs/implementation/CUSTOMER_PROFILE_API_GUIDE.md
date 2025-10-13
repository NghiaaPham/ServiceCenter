# ?? CUSTOMER PROFILE API - H??NG D?N CHI TI?T CHO FRONTEND

> **M?c ?ích:** Document này h??ng d?n chi ti?t các API endpoints liên quan ??n **Customer Profile** - Qu?n lý h? s? khách hàng.

---

## ?? T?NG QUAN

Module **Customer Profile** cho phép khách hàng:
- ? Xem thông tin cá nhân
- ? C?p nh?t thông tin cá nhân
- ? Qu?n lý xe c?a mình
- ? Xem l?ch s? ?i?m th??ng
- ? Theo dõi lo?i khách hàng (New/Regular/VIP)

---

## ?? AUTHORIZATION

**T?t c? API trong ph?n này yêu c?u:**
- ? JWT Token (?ã ??ng nh?p)
- ? Role: **Customer** (ch? khách hàng)
- ? Customer ch? xem/s?a ???c profile c?a chính mình

```http
Authorization: Bearer {token_from_login}
```

---

## ?? BASE URL

```
https://localhost:5001/api/customer/profile
```

---

## ?? DANH SÁCH API ENDPOINTS

### 1?? **Xem thông tin h? s? c?a tôi**

```http
GET /api/customer/profile/me
```

**Mô t?:** L?y toàn b? thông tin profile c?a customer ?ang ??ng nh?p

**Authorization:** Required (Customer role)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "L?y thông tin thành công",
  "data": {
    "customerId": 45,
    "customerCode": "CUST202510001",
    "userId": 123,
    "username": "customer123",
    "email": "customer@example.com",
    "fullName": "Nguy?n V?n A",
    "phoneNumber": "0912345678",
    "address": "123 ???ng ABC, Hà N?i",
    "dateOfBirth": "1990-01-15",
    "gender": "Male",
    "identityNumber": "001234567890",
    "loyaltyPoints": 1000,
    "totalSpent": 5000000,
    "customerTypeId": 2,
    "customerTypeName": "Regular",
    "customerTypeDiscount": 5,
    "preferredLanguage": "vi",
    "marketingOptIn": true,
    "emailVerified": true,
    "isActive": true,
    "createdDate": "2025-01-01T00:00:00Z",
    "lastModifiedDate": "2025-10-03T10:30:00Z"
  }
}
```

**Các tr??ng quan tr?ng:**

| Field | Type | Description |
|-------|------|-------------|
| `customerId` | int | ID khách hàng (dùng cho các API khác) |
| `customerCode` | string | Mã khách hàng (hi?n th? cho user) |
| `loyaltyPoints` | int | ?i?m th??ng tích l?y |
| `totalSpent` | decimal | T?ng chi tiêu (VN?) |
| `customerTypeName` | string | New / Regular / VIP |
| `customerTypeDiscount` | decimal | % gi?m giá theo lo?i khách hàng |
| `emailVerified` | bool | Email ?ã xác th?c ch?a |

**Use Case Frontend:**
- Hi?n th? trên trang **My Profile**
- Hi?n th? badge **VIP/Regular/New** 
- Hi?n th? ?i?m th??ng trong header
- Auto-fill form khi ??t l?ch

**Error Responses:**

```json
// 401 - Ch?a ??ng nh?p
{
  "success": false,
  "message": "Unauthorized. Vui lòng ??ng nh?p."
}

// 404 - Không tìm th?y customer
{
  "success": false,
  "message": "Không tìm th?y thông tin khách hàng"
}
```

---

### 2?? **C?p nh?t thông tin h? s?**

```http
PUT /api/customer/profile/me
```

**Mô t?:** Customer t? c?p nh?t thông tin cá nhân

**Authorization:** Required (Customer role)

**Request Body:**

```json
{
  "fullName": "Nguy?n V?n B",
  "phoneNumber": "0987654321",
  "address": "456 ???ng XYZ, TP.HCM",
  "dateOfBirth": "1990-01-15",
  "gender": "Male",
  "preferredLanguage": "vi",
  "marketingOptIn": true
}
```

**Validation Rules:**

| Field | Required | Rules |
|-------|----------|-------|
| `fullName` | ? Yes | 2-100 ký t? |
| `phoneNumber` | ? Yes | Format: 0912345678 ho?c +84912345678 |
| `address` | ? No | T?i ?a 500 ký t? |
| `dateOfBirth` | ? No | yyyy-MM-dd, ph?i > 18 tu?i |
| `gender` | ? No | Male / Female / Other |
| `preferredLanguage` | ? No | vi / en |
| `marketingOptIn` | ? No | true / false |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "C?p nh?t thông tin thành công",
  "data": {
    "customerId": 45,
    "fullName": "Nguy?n V?n B",
    "phoneNumber": "0987654321",
    "address": "456 ???ng XYZ, TP.HCM",
    "lastModifiedDate": "2025-10-03T11:00:00Z"
  }
}
```

**? Các tr??ng KHÔNG TH? s?a:**
- `email` (g?n v?i User account, ph?i s?a qua admin)
- `customerCode` (t? ??ng generate)
- `loyaltyPoints` (ch? thay ??i qua giao d?ch)
- `totalSpent` (ch? thay ??i qua giao d?ch)
- `customerTypeId` (do h? th?ng t? ??ng tính)

**Error Responses:**

```json
// 400 - Validation error
{
  "success": false,
  "message": "D? li?u không h?p l?",
  "errors": {
    "phoneNumber": ["S? ?i?n tho?i không ?úng ??nh d?ng"],
    "dateOfBirth": ["Khách hàng ph?i trên 18 tu?i"]
  }
}

// 409 - Conflict (s? ?i?n tho?i ?ã t?n t?i)
{
  "success": false,
  "message": "S? ?i?n tho?i 0987654321 ?ã ???c s? d?ng b?i khách hàng khác"
}
```

**Use Case Frontend:**
- Form **Edit Profile**
- Validate phone number format tr??c khi submit
- Validate tu?i >= 18
- Show confirm dialog tr??c khi save

---

### 3?? **Xem danh sách xe c?a tôi**

```http
GET /api/customer/profile/my-vehicles
```

**Mô t?:** L?y danh sách t?t c? xe mà customer ?ã ??ng ký

**Authorization:** Required (Customer role)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Tìm th?y 2 xe",
  "data": [
    {
      "vehicleId": 10,
      "licensePlate": "30A-12345",
      "vin": "5YJSA1E14HF123456",
      "customerId": 45,
      "customerName": "Nguy?n V?n A",
      "modelId": 5,
      "modelName": "Model 3",
      "brandId": 1,
      "brandName": "Tesla",
      "color": "Tr?ng",
      "year": 2023,
      "mileage": 15000,
      "purchaseDate": "2023-05-10",
      "nextMaintenanceDate": "2025-11-01",
      "batteryCapacity": 75,
      "batteryHealthPercent": 95,
      "insuranceNumber": "INS12345",
      "insuranceExpiry": "2026-05-10",
      "registrationExpiry": "2025-05-10",
      "isActive": true,
      "createdDate": "2023-05-10T00:00:00Z"
    },
    {
      "vehicleId": 11,
      "licensePlate": "51G-67890",
      "vin": "5YJSA1E14HF789012",
      "modelName": "Model Y",
      "brandName": "Tesla",
      "color": "?en",
      "year": 2024,
      "mileage": 5000,
      "batteryHealthPercent": 98,
      "isActive": true
    }
  ]
}
```

**S?p x?p:** M?i nh?t tr??c (CreatedDate DESC)

**Use Case Frontend:**
- Hi?n th? trên trang **My Vehicles**
- Dropdown ch?n xe khi ??t l?ch
- Card hi?n th? thông tin xe
- Badge c?nh báo n?u `insuranceExpiry` ho?c `registrationExpiry` s?p h?t h?n

**Frontend Logic:**

```javascript
// Check insurance/registration expiry
const checkExpiry = (expiryDate) => {
  const today = new Date();
  const expiry = new Date(expiryDate);
  const daysLeft = Math.ceil((expiry - today) / (1000 * 60 * 60 * 24));
  
  if (daysLeft < 0) return { status: 'expired', message: '?ã h?t h?n' };
  if (daysLeft <= 30) return { status: 'warning', message: `Còn ${daysLeft} ngày` };
  return { status: 'ok', message: 'Còn h?n' };
};

// Check battery health
const getBatteryStatus = (percent) => {
  if (percent >= 90) return 'excellent';
  if (percent >= 80) return 'good';
  if (percent >= 70) return 'fair';
  return 'poor';
};
```

---

### 4?? **??ng ký xe m?i**

```http
POST /api/customer/profile/my-vehicles
```

**Mô t?:** Customer ??ng ký thêm xe m?i vào h? th?ng

**Authorization:** Required (Customer role)

**Request Body:**

```json
{
  "modelId": 5,
  "licensePlate": "30A-99999",
  "vin": "5YJSA1E14HF999999",
  "color": "?en",
  "year": 2024,
  "purchaseDate": "2024-10-01",
  "mileage": 5000,
  "batteryCapacity": 75,
  "batteryHealthPercent": 100,
  "insuranceNumber": "INS99999",
  "insuranceExpiry": "2026-10-01",
  "registrationExpiry": "2025-10-01"
}
```

**Validation Rules:**

| Field | Required | Rules |
|-------|----------|-------|
| `modelId` | ? Yes | Ph?i t?n t?i trong h? th?ng |
| `licensePlate` | ? Yes | Format: 30A-12345, ph?i unique |
| `vin` | ? Yes | 17 ký t?, ph?i unique |
| `color` | ? No | T?i ?a 50 ký t? |
| `year` | ? No | 1900 - n?m hi?n t?i |
| `mileage` | ? No | >= 0 |
| `batteryCapacity` | ? No | kWh |
| `batteryHealthPercent` | ? No | 0-100 |
| `insuranceExpiry` | ? No | yyyy-MM-dd |
| `registrationExpiry` | ? No | yyyy-MM-dd |

**Response (201 Created):**

```json
{
  "success": true,
  "message": "??ng ký xe thành công",
  "data": {
    "vehicleId": 20,
    "licensePlate": "30A-99999",
    "vin": "5YJSA1E14HF999999",
    "modelName": "Model 3",
    "brandName": "Tesla",
    "color": "?en",
    "createdDate": "2025-10-03T11:30:00Z"
  }
}
```

**Error Responses:**

```json
// 400 - License plate ?ã t?n t?i
{
  "success": false,
  "message": "Bi?n s? xe 30A-99999 ?ã t?n t?i trong h? th?ng"
}

// 400 - VIN ?ã t?n t?i
{
  "success": false,
  "message": "Mã VIN 5YJSA1E14HF999999 ?ã ???c ??ng ký"
}

// 404 - Model không t?n t?i
{
  "success": false,
  "message": "Không tìm th?y model v?i ID 999"
}
```

**Use Case Frontend:**
- Form **Add New Vehicle**
- Dropdown ch?n Brand ? load Models theo Brand
- Validate license plate format (Vietnam)
- Validate VIN (17 ký t?)
- Upload ?nh xe (n?u có)

**Frontend Flow:**

```
1. Ch?n hãng xe (Brand) ? GET /api/lookup/car-brands
2. Ch?n model (Model) ? GET /api/lookup/car-models/by-brand/{brandId}
3. Nh?p thông tin xe
4. Submit ? POST /api/customer/profile/my-vehicles
5. Redirect v? My Vehicles
```

---

### 5?? **Xem chi ti?t 1 xe**

```http
GET /api/customer/profile/my-vehicles/{vehicleId}
```

**Mô t?:** L?y thông tin chi ti?t c?a 1 xe c? th?

**Authorization:** Required (Customer role)

**Path Parameters:**
- `vehicleId` (int, required): ID c?a xe

**Response (200 OK):**

```json
{
  "success": true,
  "message": "L?y thông tin xe thành công",
  "data": {
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "vin": "5YJSA1E14HF123456",
    "customerId": 45,
    "customerName": "Nguy?n V?n A",
    "modelId": 5,
    "modelName": "Model 3",
    "brandId": 1,
    "brandName": "Tesla",
    "color": "Tr?ng",
    "year": 2023,
    "mileage": 15000,
    "purchaseDate": "2023-05-10",
    "nextMaintenanceDate": "2025-11-01",
    "batteryCapacity": 75,
    "batteryHealthPercent": 95,
    "insuranceNumber": "INS12345",
    "insuranceExpiry": "2026-05-10",
    "registrationExpiry": "2025-05-10",
    "isActive": true,
    "createdDate": "2023-05-10T00:00:00Z",
    "lastModifiedDate": "2025-10-03T10:00:00Z",
    "statistics": {
      "totalAppointments": 10,
      "completedAppointments": 8,
      "activeSubscriptions": 1,
      "totalSpent": 15000000,
      "lastServiceDate": "2025-09-15"
    }
  }
}
```

**Error Responses:**

```json
// 403 - Xe không thu?c v? customer này
{
  "success": false,
  "message": "B?n không có quy?n xem xe này"
}

// 404 - Xe không t?n t?i
{
  "success": false,
  "message": "Không tìm th?y xe v?i ID 10"
}
```

**Use Case Frontend:**
- Trang **Vehicle Detail**
- Hi?n th? l?ch s? b?o d??ng
- Hi?n th? subscription ?ang active
- Nút **Book Appointment** cho xe này

---

### 6?? **C?p nh?t thông tin xe**

```http
PUT /api/customer/profile/my-vehicles/{vehicleId}
```

**Mô t?:** Customer c?p nh?t thông tin xe (mileage, battery health, insurance...)

**Authorization:** Required (Customer role)

**Path Parameters:**
- `vehicleId` (int, required): ID c?a xe

**Request Body:**

```json
{
  "color": "Xanh",
  "mileage": 16000,
  "batteryHealthPercent": 94,
  "insuranceNumber": "INS12345-NEW",
  "insuranceExpiry": "2027-05-10",
  "registrationExpiry": "2026-05-10"
}
```

**Các tr??ng có th? s?a:**
- ? `color`
- ? `mileage` (ch? t?ng, không gi?m)
- ? `batteryHealthPercent`
- ? `insuranceNumber`
- ? `insuranceExpiry`
- ? `registrationExpiry`

**Các tr??ng KHÔNG TH? s?a:**
- ? `licensePlate` (không cho ??i)
- ? `vin` (không cho ??i)
- ? `modelId` (không cho ??i)
- ? `customerId` (không cho ??i)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "C?p nh?t thông tin xe thành công",
  "data": {
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "color": "Xanh",
    "mileage": 16000,
    "batteryHealthPercent": 94,
    "lastModifiedDate": "2025-10-03T12:00:00Z"
  }
}
```

**Use Case Frontend:**
- Form **Update Vehicle**
- Ch? cho s?a các field ???c phép
- Validate mileage ph?i >= giá tr? c?

---

### 7?? **Ki?m tra xe có th? xóa không**

```http
GET /api/customer/profile/my-vehicles/{vehicleId}/can-delete
```

**Mô t?:** Ki?m tra xe có th? xóa hay không (d?a vào appointments, work orders, subscriptions)

**Authorization:** Required (Customer role)

**Path Parameters:**
- `vehicleId` (int, required): ID c?a xe

**Response (200 OK) - CÓ TH? XÓA:**

```json
{
  "success": true,
  "message": "Xe có th? ???c xóa",
  "data": {
    "canDelete": true,
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "reason": null
  }
}
```

**Response (200 OK) - KHÔNG TH? XÓA:**

```json
{
  "success": true,
  "message": "Xe không th? xóa",
  "data": {
    "canDelete": false,
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "reason": "Xe ?ang có l?ch h?n, phi?u công vi?c ho?c gói d?ch v? ?ang ho?t ??ng",
    "details": {
      "activeAppointments": 2,
      "openWorkOrders": 1,
      "activeSubscriptions": 1
    }
  }
}
```

**Business Rules:**

Xe **KHÔNG TH? XÓA** n?u:
- ? Có l?ch h?n ?ang active (Pending, Confirmed, CheckedIn, InProgress)
- ? Có Work Order ?ang m? (InProgress, Pending)
- ? Có Package Subscription ?ang active

**Use Case Frontend:**
- G?i API này **TR??C KHI** hi?n th? nút Delete
- N?u `canDelete = false`: disable nút Delete, hi?n th? tooltip v?i `reason`
- N?u `canDelete = true`: enable nút Delete

```javascript
// Example usage
const checkCanDelete = async (vehicleId) => {
  const response = await api.get(`/customer/profile/my-vehicles/${vehicleId}/can-delete`);
  
  if (response.data.canDelete) {
    // Show delete button
    setShowDeleteButton(true);
  } else {
    // Show disabled button with tooltip
    setShowDeleteButton(false);
    setDeleteDisabledReason(response.data.reason);
  }
};
```

---

### 8?? **Xóa xe c?a tôi**

```http
DELETE /api/customer/profile/my-vehicles/{vehicleId}
```

**Mô t?:** Xóa xe kh?i danh sách (soft delete)

**Authorization:** Required (Customer role)

**Path Parameters:**
- `vehicleId` (int, required): ID c?a xe

**Response (200 OK):**

```json
{
  "success": true,
  "message": "?ã xóa xe 30A-12345 kh?i danh sách c?a b?n",
  "data": {
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "deletedAt": "2025-10-03T12:30:00Z"
  }
}
```

**Error Responses:**

```json
// 400 - Xe không th? xóa
{
  "success": false,
  "message": "Không th? xóa xe này",
  "errorCode": "VEHICLE_HAS_DEPENDENCIES",
  "data": {
    "reason": "Xe ?ang có l?ch h?n, phi?u công vi?c ho?c gói d?ch v? ?ang ho?t ??ng",
    "activeAppointments": 2,
    "openWorkOrders": 1,
    "activeSubscriptions": 1
  }
}

// 403 - Xe không thu?c v? customer này
{
  "success": false,
  "message": "B?n không có quy?n xóa xe này"
}
```

**?? L?U Ý QUAN TR?NG:**
- ?ây là **soft delete** (?ánh d?u `IsActive = false`, không xóa v?t lý)
- Ph?i g?i API `can-delete` tr??c ?? check
- Hi?n th? confirm dialog tr??c khi xóa

**Use Case Frontend:**

```javascript
const handleDeleteVehicle = async (vehicleId) => {
  // 1. Check can delete
  const checkResponse = await api.get(`/customer/profile/my-vehicles/${vehicleId}/can-delete`);
  
  if (!checkResponse.data.canDelete) {
    alert(checkResponse.data.reason);
    return;
  }
  
  // 2. Confirm dialog
  const confirmed = await showConfirmDialog({
    title: 'Xóa xe',
    message: 'B?n có ch?c ch?n mu?n xóa xe này?',
    confirmText: 'Xóa',
    cancelText: 'H?y'
  });
  
  if (!confirmed) return;
  
  // 3. Delete
  try {
    await api.delete(`/customer/profile/my-vehicles/${vehicleId}`);
    showSuccessToast('?ã xóa xe thành công');
    // Reload vehicle list
    fetchVehicles();
  } catch (error) {
    showErrorToast(error.response.data.message);
  }
};
```

---

## ?? UI/UX RECOMMENDATIONS

### 1. **Profile Page Layout**

```
???????????????????????????????????????????
?  ?? My Profile                          ?
???????????????????????????????????????????
?                                         ?
?  ?? Personal Information                ?
?  ?? Full Name: Nguy?n V?n A            ?
?  ?? Email: customer@example.com        ?
?  ?? Phone: 0912345678                  ?
?  ?? Address: 123 ???ng ABC, Hà N?i     ?
?  ?? [Edit Profile] button              ?
?                                         ?
?  ?? Customer Status                     ?
?  ?? Type: ?? VIP (5% discount)         ?
?  ?? Loyalty Points: 1,000 ?i?m         ?
?  ?? Total Spent: 5,000,000 VN?         ?
?                                         ?
?  ?? My Vehicles (2)                     ?
?  ?? Tesla Model 3 - 30A-12345          ?
?  ?? Tesla Model Y - 51G-67890          ?
?  ?? [+ Add Vehicle] button             ?
?                                         ?
???????????????????????????????????????????
```

### 2. **Vehicle Card Component**

```jsx
<VehicleCard>
  <VehicleImage src={model.logoUrl} />
  <VehicleInfo>
    <Brand>{brandName}</Brand>
    <Model>{modelName}</Model>
    <LicensePlate>{licensePlate}</LicensePlate>
  </VehicleInfo>
  <VehicleStats>
    <Battery health={batteryHealthPercent} />
    <Mileage>{mileage} km</Mileage>
    <Insurance expiry={insuranceExpiry} />
  </VehicleStats>
  <Actions>
    <Button onClick={() => viewDetail(vehicleId)}>Chi ti?t</Button>
    <Button onClick={() => bookAppointment(vehicleId)}>??t l?ch</Button>
    <Button onClick={() => deleteVehicle(vehicleId)} disabled={!canDelete}>Xóa</Button>
  </Actions>
</VehicleCard>
```

### 3. **Customer Type Badge**

```jsx
const CustomerTypeBadge = ({ typeName, discount }) => {
  const badgeConfig = {
    'New': { color: 'gray', icon: '??' },
    'Regular': { color: 'blue', icon: '?' },
    'VIP': { color: 'gold', icon: '??' }
  };
  
  const config = badgeConfig[typeName];
  
  return (
    <Badge color={config.color}>
      {config.icon} {typeName} ({discount}% discount)
    </Badge>
  );
};
```

---

## ?? DATA FLOW DIAGRAMS

### Flow 1: View Profile

```
???????????      GET /profile/me      ???????????
?         ? ?????????????????????????> ?         ?
? Client  ?                            ?  Server ?
?         ? <????????????????????????? ?         ?
???????????   200 OK + Profile Data   ???????????
     ?
     ??> Display profile info
     ??> Show customer type badge
     ??> Show loyalty points
     ??> List vehicles
```

### Flow 2: Update Profile

```
???????????    PUT /profile/me       ???????????
?         ? ?????????????????????????> ?         ?
? Client  ?   (updated data)           ?  Server ?
?         ?                            ?         ?
?         ? <????????????????????????? ?         ?
???????????   200 OK / 400 Error      ???????????
     ?
     ?? Success: Show success message, reload profile
     ?? Error: Show validation errors
```

### Flow 3: Add Vehicle

```
???????????                           ???????????
? Client  ?  1. GET /lookup/brands    ?  Server ?
?         ? ?????????????????????????> ?         ?
?         ? <????????????????????????? ?         ?
?         ?   [Tesla, BYD, ...]        ?         ?
?         ?                            ?         ?
?         ?  2. GET /lookup/models/1   ?         ?
?         ? ?????????????????????????> ?         ?
?         ? <????????????????????????? ?         ?
?         ?   [Model 3, Model Y, ...]  ?         ?
?         ?                            ?         ?
?         ?  3. POST /my-vehicles      ?         ?
?         ? ?????????????????????????> ?         ?
?         ?   (vehicle data)           ?         ?
?         ? <????????????????????????? ?         ?
???????????   201 Created             ???????????
     ?
     ??> Redirect to My Vehicles page
```

### Flow 4: Delete Vehicle

```
???????????                              ???????????
? Client  ?  1. GET /vehicles/10/can-delete ? Server ?
?         ? ??????????????????????????????> ?         ?
?         ? <?????????????????????????????? ?         ?
?         ?   { canDelete: true }           ?         ?
?         ?                                 ?         ?
?         ?  2. [User confirms]             ?         ?
?         ?                                 ?         ?
?         ?  3. DELETE /vehicles/10         ?         ?
?         ? ??????????????????????????????> ?         ?
?         ? <?????????????????????????????? ?         ?
???????????   200 OK                        ???????????
     ?
     ??> Reload vehicle list
```

---

## ?? SECURITY NOTES

### 1. **Authorization Check**

Backend s? t? ??ng ki?m tra:
- ? JWT token h?p l?
- ? User có role Customer
- ? Customer ch? xem/s?a ???c data c?a mình

Frontend **KHÔNG C?N** g?i `customerId` trong request body. Backend s? t? l?y t? JWT token.

```javascript
// ? WRONG - Không c?n g?i customerId
const data = {
  customerId: 45,  // Backend s? ignore field này
  fullName: "Nguy?n V?n A"
};

// ? CORRECT
const data = {
  fullName: "Nguy?n V?n A"
};
```

### 2. **Token Refresh**

N?u API tr? v? `401 Unauthorized`:

```javascript
axios.interceptors.response.use(
  response => response,
  async error => {
    if (error.response.status === 401) {
      // Clear token
      localStorage.removeItem('authToken');
      // Redirect to login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

---

## ?? TESTING CHECKLIST

### Profile APIs

- [ ] `GET /profile/me` - Load profile thành công
- [ ] `GET /profile/me` - Tr? 401 n?u không ??ng nh?p
- [ ] `PUT /profile/me` - Update thành công
- [ ] `PUT /profile/me` - Validate phone number format
- [ ] `PUT /profile/me` - Validate tu?i >= 18
- [ ] `PUT /profile/me` - Không cho s?a email, customerCode

### Vehicle APIs

- [ ] `GET /my-vehicles` - List all vehicles
- [ ] `GET /my-vehicles` - Return empty array n?u ch?a có xe
- [ ] `POST /my-vehicles` - Add vehicle thành công
- [ ] `POST /my-vehicles` - Validate license plate unique
- [ ] `POST /my-vehicles` - Validate VIN unique
- [ ] `GET /my-vehicles/{id}` - Get detail thành công
- [ ] `GET /my-vehicles/{id}` - Return 403 n?u xe không thu?c v? customer
- [ ] `PUT /my-vehicles/{id}` - Update vehicle thành công
- [ ] `PUT /my-vehicles/{id}` - Không cho gi?m mileage
- [ ] `GET /my-vehicles/{id}/can-delete` - Return true n?u có th? xóa
- [ ] `GET /my-vehicles/{id}/can-delete` - Return false n?u có dependencies
- [ ] `DELETE /my-vehicles/{id}` - Delete thành công
- [ ] `DELETE /my-vehicles/{id}` - Return 400 n?u có dependencies

---

## ?? RELATED APIS

Các API khác liên quan ??n Customer Profile:

### Lookup APIs (?? build form)

```http
GET /api/lookup/car-brands
GET /api/lookup/car-models/by-brand/{brandId}
GET /api/customer-types
```

### Appointment APIs (dùng vehicle)

```http
POST /api/appointments
  -> C?n vehicleId t? my-vehicles

GET /api/appointments/my-appointments
  -> Filter theo vehicleId
```

### Package Subscription APIs (dùng vehicle)

```http
GET /api/package-subscriptions/vehicle/{vehicleId}/active
POST /api/package-subscriptions/purchase
  -> C?n vehicleId
```

---

## ?? BEST PRACTICES

### 1. **Caching Profile Data**

```javascript
// Cache profile data ?? gi?m API calls
const useProfile = () => {
  const [profile, setProfile] = useState(null);
  
  useEffect(() => {
    const cachedProfile = localStorage.getItem('userProfile');
    if (cachedProfile) {
      setProfile(JSON.parse(cachedProfile));
    }
    
    // Refresh from server
    fetchProfile().then(data => {
      setProfile(data);
      localStorage.setItem('userProfile', JSON.stringify(data));
    });
  }, []);
  
  return profile;
};
```

### 2. **Optimistic Updates**

```javascript
const updateProfile = async (data) => {
  // Update UI immediately
  setProfile(prev => ({ ...prev, ...data }));
  
  try {
    // Call API
    const response = await api.put('/profile/me', data);
    // Update with server response
    setProfile(response.data);
  } catch (error) {
    // Rollback on error
    setProfile(originalProfile);
    showError(error.message);
  }
};
```

### 3. **Form Validation**

```javascript
const validateProfileForm = (data) => {
  const errors = {};
  
  // Phone validation
  if (!/^0\d{9}$/.test(data.phoneNumber)) {
    errors.phoneNumber = 'S? ?i?n tho?i không h?p l?';
  }
  
  // Age validation
  const age = calculateAge(data.dateOfBirth);
  if (age < 18) {
    errors.dateOfBirth = 'Ph?i trên 18 tu?i';
  }
  
  return errors;
};
```

---

## ?? COMMON ERRORS & SOLUTIONS

| Error | Cause | Solution |
|-------|-------|----------|
| 401 Unauthorized | Token h?t h?n ho?c không h?p l? | Redirect v? login page |
| 403 Forbidden | Customer c? xem/s?a data c?a ng??i khác | Show error message, prevent action |
| 400 Bad Request | Validation failed | Show validation errors trên form |
| 409 Conflict | License plate/VIN ?ã t?n t?i | Show error, suggest user check l?i |
| 404 Not Found | Vehicle không t?n t?i | Redirect v? vehicle list |

---

## ?? H? TR?

N?u có v?n ?? khi integrate:

1. ? Check Swagger UI: `https://localhost:5001/swagger`
2. ? Check API logs trong Output Window
3. ? Contact Backend Team Lead

---

**Document Version:** 1.0  
**Last Updated:** 2025-10-03  
**Author:** Backend Team  
**For:** Frontend Team
