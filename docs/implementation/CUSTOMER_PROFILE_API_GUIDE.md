# ?? CUSTOMER PROFILE API - H??NG D?N CHI TI?T CHO FRONTEND

> **M?c ?�ch:** Document n�y h??ng d?n chi ti?t c�c API endpoints li�n quan ??n **Customer Profile** - Qu?n l� h? s? kh�ch h�ng.

---

## ?? T?NG QUAN

Module **Customer Profile** cho ph�p kh�ch h�ng:
- ? Xem th�ng tin c� nh�n
- ? C?p nh?t th�ng tin c� nh�n
- ? Qu?n l� xe c?a m�nh
- ? Xem l?ch s? ?i?m th??ng
- ? Theo d�i lo?i kh�ch h�ng (New/Regular/VIP)

---

## ?? AUTHORIZATION

**T?t c? API trong ph?n n�y y�u c?u:**
- ? JWT Token (?� ??ng nh?p)
- ? Role: **Customer** (ch? kh�ch h�ng)
- ? Customer ch? xem/s?a ???c profile c?a ch�nh m�nh

```http
Authorization: Bearer {token_from_login}
```

---

## ?? BASE URL

```
https://localhost:5001/api/customer/profile
```

---

## ?? DANH S�CH API ENDPOINTS

### 1?? **Xem th�ng tin h? s? c?a t�i**

```http
GET /api/customer/profile/me
```

**M� t?:** L?y to�n b? th�ng tin profile c?a customer ?ang ??ng nh?p

**Authorization:** Required (Customer role)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "L?y th�ng tin th�nh c�ng",
  "data": {
    "customerId": 45,
    "customerCode": "CUST202510001",
    "userId": 123,
    "username": "customer123",
    "email": "customer@example.com",
    "fullName": "Nguy?n V?n A",
    "phoneNumber": "0912345678",
    "address": "123 ???ng ABC, H� N?i",
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

**C�c tr??ng quan tr?ng:**

| Field | Type | Description |
|-------|------|-------------|
| `customerId` | int | ID kh�ch h�ng (d�ng cho c�c API kh�c) |
| `customerCode` | string | M� kh�ch h�ng (hi?n th? cho user) |
| `loyaltyPoints` | int | ?i?m th??ng t�ch l?y |
| `totalSpent` | decimal | T?ng chi ti�u (VN?) |
| `customerTypeName` | string | New / Regular / VIP |
| `customerTypeDiscount` | decimal | % gi?m gi� theo lo?i kh�ch h�ng |
| `emailVerified` | bool | Email ?� x�c th?c ch?a |

**Use Case Frontend:**
- Hi?n th? tr�n trang **My Profile**
- Hi?n th? badge **VIP/Regular/New** 
- Hi?n th? ?i?m th??ng trong header
- Auto-fill form khi ??t l?ch

**Error Responses:**

```json
// 401 - Ch?a ??ng nh?p
{
  "success": false,
  "message": "Unauthorized. Vui l�ng ??ng nh?p."
}

// 404 - Kh�ng t�m th?y customer
{
  "success": false,
  "message": "Kh�ng t�m th?y th�ng tin kh�ch h�ng"
}
```

---

### 2?? **C?p nh?t th�ng tin h? s?**

```http
PUT /api/customer/profile/me
```

**M� t?:** Customer t? c?p nh?t th�ng tin c� nh�n

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
| `fullName` | ? Yes | 2-100 k� t? |
| `phoneNumber` | ? Yes | Format: 0912345678 ho?c +84912345678 |
| `address` | ? No | T?i ?a 500 k� t? |
| `dateOfBirth` | ? No | yyyy-MM-dd, ph?i > 18 tu?i |
| `gender` | ? No | Male / Female / Other |
| `preferredLanguage` | ? No | vi / en |
| `marketingOptIn` | ? No | true / false |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "C?p nh?t th�ng tin th�nh c�ng",
  "data": {
    "customerId": 45,
    "fullName": "Nguy?n V?n B",
    "phoneNumber": "0987654321",
    "address": "456 ???ng XYZ, TP.HCM",
    "lastModifiedDate": "2025-10-03T11:00:00Z"
  }
}
```

**? C�c tr??ng KH�NG TH? s?a:**
- `email` (g?n v?i User account, ph?i s?a qua admin)
- `customerCode` (t? ??ng generate)
- `loyaltyPoints` (ch? thay ??i qua giao d?ch)
- `totalSpent` (ch? thay ??i qua giao d?ch)
- `customerTypeId` (do h? th?ng t? ??ng t�nh)

**Error Responses:**

```json
// 400 - Validation error
{
  "success": false,
  "message": "D? li?u kh�ng h?p l?",
  "errors": {
    "phoneNumber": ["S? ?i?n tho?i kh�ng ?�ng ??nh d?ng"],
    "dateOfBirth": ["Kh�ch h�ng ph?i tr�n 18 tu?i"]
  }
}

// 409 - Conflict (s? ?i?n tho?i ?� t?n t?i)
{
  "success": false,
  "message": "S? ?i?n tho?i 0987654321 ?� ???c s? d?ng b?i kh�ch h�ng kh�c"
}
```

**Use Case Frontend:**
- Form **Edit Profile**
- Validate phone number format tr??c khi submit
- Validate tu?i >= 18
- Show confirm dialog tr??c khi save

---

### 3?? **Xem danh s�ch xe c?a t�i**

```http
GET /api/customer/profile/my-vehicles
```

**M� t?:** L?y danh s�ch t?t c? xe m� customer ?� ??ng k�

**Authorization:** Required (Customer role)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "T�m th?y 2 xe",
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
- Hi?n th? tr�n trang **My Vehicles**
- Dropdown ch?n xe khi ??t l?ch
- Card hi?n th? th�ng tin xe
- Badge c?nh b�o n?u `insuranceExpiry` ho?c `registrationExpiry` s?p h?t h?n

**Frontend Logic:**

```javascript
// Check insurance/registration expiry
const checkExpiry = (expiryDate) => {
  const today = new Date();
  const expiry = new Date(expiryDate);
  const daysLeft = Math.ceil((expiry - today) / (1000 * 60 * 60 * 24));
  
  if (daysLeft < 0) return { status: 'expired', message: '?� h?t h?n' };
  if (daysLeft <= 30) return { status: 'warning', message: `C�n ${daysLeft} ng�y` };
  return { status: 'ok', message: 'C�n h?n' };
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

### 4?? **??ng k� xe m?i**

```http
POST /api/customer/profile/my-vehicles
```

**M� t?:** Customer ??ng k� th�m xe m?i v�o h? th?ng

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
| `vin` | ? Yes | 17 k� t?, ph?i unique |
| `color` | ? No | T?i ?a 50 k� t? |
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
  "message": "??ng k� xe th�nh c�ng",
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
// 400 - License plate ?� t?n t?i
{
  "success": false,
  "message": "Bi?n s? xe 30A-99999 ?� t?n t?i trong h? th?ng"
}

// 400 - VIN ?� t?n t?i
{
  "success": false,
  "message": "M� VIN 5YJSA1E14HF999999 ?� ???c ??ng k�"
}

// 404 - Model kh�ng t?n t?i
{
  "success": false,
  "message": "Kh�ng t�m th?y model v?i ID 999"
}
```

**Use Case Frontend:**
- Form **Add New Vehicle**
- Dropdown ch?n Brand ? load Models theo Brand
- Validate license plate format (Vietnam)
- Validate VIN (17 k� t?)
- Upload ?nh xe (n?u c�)

**Frontend Flow:**

```
1. Ch?n h�ng xe (Brand) ? GET /api/lookup/car-brands
2. Ch?n model (Model) ? GET /api/lookup/car-models/by-brand/{brandId}
3. Nh?p th�ng tin xe
4. Submit ? POST /api/customer/profile/my-vehicles
5. Redirect v? My Vehicles
```

---

### 5?? **Xem chi ti?t 1 xe**

```http
GET /api/customer/profile/my-vehicles/{vehicleId}
```

**M� t?:** L?y th�ng tin chi ti?t c?a 1 xe c? th?

**Authorization:** Required (Customer role)

**Path Parameters:**
- `vehicleId` (int, required): ID c?a xe

**Response (200 OK):**

```json
{
  "success": true,
  "message": "L?y th�ng tin xe th�nh c�ng",
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
// 403 - Xe kh�ng thu?c v? customer n�y
{
  "success": false,
  "message": "B?n kh�ng c� quy?n xem xe n�y"
}

// 404 - Xe kh�ng t?n t?i
{
  "success": false,
  "message": "Kh�ng t�m th?y xe v?i ID 10"
}
```

**Use Case Frontend:**
- Trang **Vehicle Detail**
- Hi?n th? l?ch s? b?o d??ng
- Hi?n th? subscription ?ang active
- N�t **Book Appointment** cho xe n�y

---

### 6?? **C?p nh?t th�ng tin xe**

```http
PUT /api/customer/profile/my-vehicles/{vehicleId}
```

**M� t?:** Customer c?p nh?t th�ng tin xe (mileage, battery health, insurance...)

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

**C�c tr??ng c� th? s?a:**
- ? `color`
- ? `mileage` (ch? t?ng, kh�ng gi?m)
- ? `batteryHealthPercent`
- ? `insuranceNumber`
- ? `insuranceExpiry`
- ? `registrationExpiry`

**C�c tr??ng KH�NG TH? s?a:**
- ? `licensePlate` (kh�ng cho ??i)
- ? `vin` (kh�ng cho ??i)
- ? `modelId` (kh�ng cho ??i)
- ? `customerId` (kh�ng cho ??i)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "C?p nh?t th�ng tin xe th�nh c�ng",
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
- Ch? cho s?a c�c field ???c ph�p
- Validate mileage ph?i >= gi� tr? c?

---

### 7?? **Ki?m tra xe c� th? x�a kh�ng**

```http
GET /api/customer/profile/my-vehicles/{vehicleId}/can-delete
```

**M� t?:** Ki?m tra xe c� th? x�a hay kh�ng (d?a v�o appointments, work orders, subscriptions)

**Authorization:** Required (Customer role)

**Path Parameters:**
- `vehicleId` (int, required): ID c?a xe

**Response (200 OK) - C� TH? X�A:**

```json
{
  "success": true,
  "message": "Xe c� th? ???c x�a",
  "data": {
    "canDelete": true,
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "reason": null
  }
}
```

**Response (200 OK) - KH�NG TH? X�A:**

```json
{
  "success": true,
  "message": "Xe kh�ng th? x�a",
  "data": {
    "canDelete": false,
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "reason": "Xe ?ang c� l?ch h?n, phi?u c�ng vi?c ho?c g�i d?ch v? ?ang ho?t ??ng",
    "details": {
      "activeAppointments": 2,
      "openWorkOrders": 1,
      "activeSubscriptions": 1
    }
  }
}
```

**Business Rules:**

Xe **KH�NG TH? X�A** n?u:
- ? C� l?ch h?n ?ang active (Pending, Confirmed, CheckedIn, InProgress)
- ? C� Work Order ?ang m? (InProgress, Pending)
- ? C� Package Subscription ?ang active

**Use Case Frontend:**
- G?i API n�y **TR??C KHI** hi?n th? n�t Delete
- N?u `canDelete = false`: disable n�t Delete, hi?n th? tooltip v?i `reason`
- N?u `canDelete = true`: enable n�t Delete

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

### 8?? **X�a xe c?a t�i**

```http
DELETE /api/customer/profile/my-vehicles/{vehicleId}
```

**M� t?:** X�a xe kh?i danh s�ch (soft delete)

**Authorization:** Required (Customer role)

**Path Parameters:**
- `vehicleId` (int, required): ID c?a xe

**Response (200 OK):**

```json
{
  "success": true,
  "message": "?� x�a xe 30A-12345 kh?i danh s�ch c?a b?n",
  "data": {
    "vehicleId": 10,
    "licensePlate": "30A-12345",
    "deletedAt": "2025-10-03T12:30:00Z"
  }
}
```

**Error Responses:**

```json
// 400 - Xe kh�ng th? x�a
{
  "success": false,
  "message": "Kh�ng th? x�a xe n�y",
  "errorCode": "VEHICLE_HAS_DEPENDENCIES",
  "data": {
    "reason": "Xe ?ang c� l?ch h?n, phi?u c�ng vi?c ho?c g�i d?ch v? ?ang ho?t ??ng",
    "activeAppointments": 2,
    "openWorkOrders": 1,
    "activeSubscriptions": 1
  }
}

// 403 - Xe kh�ng thu?c v? customer n�y
{
  "success": false,
  "message": "B?n kh�ng c� quy?n x�a xe n�y"
}
```

**?? L?U � QUAN TR?NG:**
- ?�y l� **soft delete** (?�nh d?u `IsActive = false`, kh�ng x�a v?t l�)
- Ph?i g?i API `can-delete` tr??c ?? check
- Hi?n th? confirm dialog tr??c khi x�a

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
    title: 'X�a xe',
    message: 'B?n c� ch?c ch?n mu?n x�a xe n�y?',
    confirmText: 'X�a',
    cancelText: 'H?y'
  });
  
  if (!confirmed) return;
  
  // 3. Delete
  try {
    await api.delete(`/customer/profile/my-vehicles/${vehicleId}`);
    showSuccessToast('?� x�a xe th�nh c�ng');
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
?  ?? Address: 123 ???ng ABC, H� N?i     ?
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
    <Button onClick={() => deleteVehicle(vehicleId)} disabled={!canDelete}>X�a</Button>
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
- ? User c� role Customer
- ? Customer ch? xem/s?a ???c data c?a m�nh

Frontend **KH�NG C?N** g?i `customerId` trong request body. Backend s? t? l?y t? JWT token.

```javascript
// ? WRONG - Kh�ng c?n g?i customerId
const data = {
  customerId: 45,  // Backend s? ignore field n�y
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

- [ ] `GET /profile/me` - Load profile th�nh c�ng
- [ ] `GET /profile/me` - Tr? 401 n?u kh�ng ??ng nh?p
- [ ] `PUT /profile/me` - Update th�nh c�ng
- [ ] `PUT /profile/me` - Validate phone number format
- [ ] `PUT /profile/me` - Validate tu?i >= 18
- [ ] `PUT /profile/me` - Kh�ng cho s?a email, customerCode

### Vehicle APIs

- [ ] `GET /my-vehicles` - List all vehicles
- [ ] `GET /my-vehicles` - Return empty array n?u ch?a c� xe
- [ ] `POST /my-vehicles` - Add vehicle th�nh c�ng
- [ ] `POST /my-vehicles` - Validate license plate unique
- [ ] `POST /my-vehicles` - Validate VIN unique
- [ ] `GET /my-vehicles/{id}` - Get detail th�nh c�ng
- [ ] `GET /my-vehicles/{id}` - Return 403 n?u xe kh�ng thu?c v? customer
- [ ] `PUT /my-vehicles/{id}` - Update vehicle th�nh c�ng
- [ ] `PUT /my-vehicles/{id}` - Kh�ng cho gi?m mileage
- [ ] `GET /my-vehicles/{id}/can-delete` - Return true n?u c� th? x�a
- [ ] `GET /my-vehicles/{id}/can-delete` - Return false n?u c� dependencies
- [ ] `DELETE /my-vehicles/{id}` - Delete th�nh c�ng
- [ ] `DELETE /my-vehicles/{id}` - Return 400 n?u c� dependencies

---

## ?? RELATED APIS

C�c API kh�c li�n quan ??n Customer Profile:

### Lookup APIs (?? build form)

```http
GET /api/lookup/car-brands
GET /api/lookup/car-models/by-brand/{brandId}
GET /api/customer-types
```

### Appointment APIs (d�ng vehicle)

```http
POST /api/appointments
  -> C?n vehicleId t? my-vehicles

GET /api/appointments/my-appointments
  -> Filter theo vehicleId
```

### Package Subscription APIs (d�ng vehicle)

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
    errors.phoneNumber = 'S? ?i?n tho?i kh�ng h?p l?';
  }
  
  // Age validation
  const age = calculateAge(data.dateOfBirth);
  if (age < 18) {
    errors.dateOfBirth = 'Ph?i tr�n 18 tu?i';
  }
  
  return errors;
};
```

---

## ?? COMMON ERRORS & SOLUTIONS

| Error | Cause | Solution |
|-------|-------|----------|
| 401 Unauthorized | Token h?t h?n ho?c kh�ng h?p l? | Redirect v? login page |
| 403 Forbidden | Customer c? xem/s?a data c?a ng??i kh�c | Show error message, prevent action |
| 400 Bad Request | Validation failed | Show validation errors tr�n form |
| 409 Conflict | License plate/VIN ?� t?n t?i | Show error, suggest user check l?i |
| 404 Not Found | Vehicle kh�ng t?n t?i | Redirect v? vehicle list |

---

## ?? H? TR?

N?u c� v?n ?? khi integrate:

1. ? Check Swagger UI: `https://localhost:5001/swagger`
2. ? Check API logs trong Output Window
3. ? Contact Backend Team Lead

---

**Document Version:** 1.0  
**Last Updated:** 2025-10-03  
**Author:** Backend Team  
**For:** Frontend Team
