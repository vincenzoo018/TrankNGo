# TrackNGo - Sidebar Module Access by Role

## Visual Guide - Complete Sidebar Navigation

This document shows **exactly what menu items each role will see** in the sidebar navigation after the comprehensive update.

---

## 🔵 Mayor (Local Chief Executive)

### Sidebar Menu:
```
┌─────────────────────────────────┐
│   TrackNGo Mati City           │
├─────────────────────────────────┤
│ 📊 Dashboard                    │
│ 📄 All Documents                │
│ 🔄 Workflow Routing             │
│ 📱 QR Tracking                  │
│ ✍️  Digital Signature  ⭐ EXCLUSIVE │
│ 📧 SMS Notifications            │
│ ⏰ ARTA Compliance              │
├─────────────────────────────────┤
│ 🚪 Logout                       │
└─────────────────────────────────┘
```

### Module Count: **7 modules**

### Module Details:
| Module | URL | Purpose |
|--------|-----|---------|
| Dashboard | `/Dashboard/Index` | View pending signatures, ARTA metrics |
| All Documents | `/Document/Index` | View all documents system-wide |
| Workflow Routing | `/Workflow/Index` | Monitor document flow, add comments |
| QR Tracking | `/Tracking/Index` | Track documents via QR code |
| **Digital Signature** | `/Signature/Index` | **Sign/Approve/Reject documents (EXCLUSIVE)** |
| SMS Notifications | `/Notification/Index` | View SMS delivery status |
| ARTA Compliance | `/Oversight/Index` | View escalated/flagged documents |

---

## 🟢 Executive Admin (Executive Assistants & Administrators)

### Sidebar Menu:
```
┌─────────────────────────────────┐
│   TrackNGo Mati City           │
├─────────────────────────────────┤
│ 📊 Dashboard                    │
│ 📄 All Documents                │
│ ➕ Document Intake              │
│ 🔄 Workflow Routing             │
│ 📧 SMS Notifications            │
│ ⏰ ARTA Compliance              │
│ 👥 User Management              │
│ ⚙️  System Configuration  ⭐ EXCLUSIVE │
│ 📝 Audit Trail                  │
├─────────────────────────────────┤
│ 🚪 Logout                       │
└─────────────────────────────────┘
```

### Module Count: **9 modules**

### Module Details:
| Module | URL | Purpose |
|--------|-----|---------|
| Dashboard | `/Dashboard/Index` | View newly submitted, pending Mayor |
| All Documents | `/Document/Index` | View all documents |
| Document Intake | `/Document/Create` | Register new documents |
| Workflow Routing | `/Workflow/Index` | Endorse, forward, submit for approval |
| SMS Notifications | `/Notification/Index` | View SMS history |
| ARTA Compliance | `/Oversight/Index` | Monitor compliance |
| User Management | `/Admin/Index` | Create users, reset passwords |
| **System Configuration** | `/Admin/Index` | **Manage departments, doc types, workflows (EXCLUSIVE)** |
| Audit Trail | `/Admin/Audit` | View system logs |

---

## 🟡 Records Officer (Records Officers & Receiving Clerks)

### Sidebar Menu:
```
┌─────────────────────────────────┐
│   TrackNGo Mati City           │
├─────────────────────────────────┤
│ 📊 Dashboard                    │
│ 📄 All Documents                │
│ ➕ Document Intake              │
│ 📱 QR Tracking                  │
│ 📧 SMS Notifications            │
├─────────────────────────────────┤
│ 🚪 Logout                       │
└─────────────────────────────────┘
```

### Module Count: **5 modules**

### Module Details:
| Module | URL | Purpose |
|--------|-----|---------|
| Dashboard | `/Dashboard/Index` | View received today, awaiting resubmission |
| All Documents | `/Document/Index` | View documents (read-only) |
| Document Intake | `/Document/Create` | Register new documents, generate QR codes |
| QR Tracking | `/Tracking/Index` | Track document progress |
| SMS Notifications | `/Notification/Index` | View SMS delivery status |

**Note**: Records Officer has the **simplest menu** with focus on document intake and tracking.

---

## 🔴 CART/Oversight Officer (Compliance, Accountability, Regulatory & Transparency)

### Sidebar Menu:
```
┌─────────────────────────────────┐
│   TrackNGo Mati City           │
├─────────────────────────────────┤
│ 📊 Dashboard                    │
│ 📄 All Documents                │
│ 📧 SMS Notifications            │
│ ⏰ ARTA Compliance              │
│ 📊 Reports & Export  ⭐ EXCLUSIVE │
│ 👥 User Management              │
│ 📝 Audit Trail                  │
├─────────────────────────────────┤
│ 🚪 Logout                       │
└─────────────────────────────────┘
```

### Module Count: **7 modules**

### Module Details:
| Module | URL | Purpose |
|--------|-----|---------|
| Dashboard | `/Dashboard/Index` | View warning/critical/overdue escalations |
| All Documents | `/Document/Index` | View all documents (read-only) |
| SMS Notifications | `/Notification/Index` | View SMS history |
| ARTA Compliance | `/Oversight/Index` | Monitor compliance, resolve escalations |
| **Reports & Export** | `/Oversight/Reports` | **Generate reports, export CSV (EXCLUSIVE)** |
| User Management | `/Admin/Index` | Create users, reset passwords |
| Audit Trail | `/Admin/Audit` | View all system logs |

**Note**: Oversight Officer has **monitoring and compliance focus** with exclusive export authority.

---

## 📊 Module Access Comparison Table

| Module | Mayor | Exec Admin | Records | Oversight |
|--------|:-----:|:----------:|:-------:|:---------:|
| Dashboard | ✅ | ✅ | ✅ | ✅ |
| All Documents | ✅ | ✅ | ✅ | ✅ |
| Document Intake | ❌ | ✅ | ✅ | ❌ |
| Workflow Routing | ✅ | ✅ | ❌ | ❌ |
| QR Tracking | ✅ | ❌ | ✅ | ❌ |
| **Digital Signature** | **✅ EXCLUSIVE** | ❌ | ❌ | ❌ |
| SMS Notifications | ✅ | ✅ | ✅ | ✅ |
| ARTA Compliance | ✅ | ✅ | ❌ | ✅ |
| **Reports & Export** | ❌ | ❌ | ❌ | **✅ EXCLUSIVE** |
| User Management | ❌ | ✅ | ❌ | ✅ |
| **System Configuration** | ❌ | **✅ EXCLUSIVE** | ❌ | ❌ |
| Audit Trail | ❌ | ✅ | ❌ | ✅ |
| **TOTAL MODULES** | **7** | **9** | **5** | **7** |

---

## 🔒 Exclusive Module Authority

### Digital Signature (Mayor ONLY)
- **URL**: `/Signature/Index`
- **Why Exclusive**: Legal authority - only the Local Chief Executive can officially sign government documents
- **Icon**: Shield with checkmark
- **Actions**: Approve, Reject, Sign documents

### Reports & Export (Oversight Officer ONLY)
- **URL**: `/Oversight/Reports`
- **Why Exclusive**: Data security - requires secondary password, COA compliance
- **Icon**: Document with charts
- **Actions**: Generate reports, export CSV

### System Configuration (Executive Admin ONLY)
- **URL**: `/Admin/Index`
- **Why Exclusive**: System integrity - manages core workflows and document types
- **Icon**: Gear/settings
- **Actions**: Configure departments, document types, workflow steps

---

## 🎨 Icon Legend

Each module has a distinct SVG icon in the sidebar:

| Icon | Module | Description |
|------|--------|-------------|
| 📊 | Dashboard | Grid of 4 squares |
| 📄 | All Documents | Document with lines |
| ➕ | Document Intake | Document with pencil/edit |
| 🔄 | Workflow Routing | Connected nodes/arrows |
| 📱 | QR Tracking | QR code pattern |
| ✍️ | Digital Signature | Shield with checkmark |
| 📧 | SMS Notifications | Envelope/mail |
| ⏰ | ARTA Compliance | Clock |
| 📊 | Reports & Export | Document with chart |
| 👥 | User Management | Person with plus |
| ⚙️ | System Configuration | Gear |
| 📝 | Audit Trail | Document with dot |
| 🚪 | Logout | Door with arrow |

---

## 🔐 Security & Access Control

### How Role Detection Works:

1. **Login Flow**:
   ```
   User Login → AuthService validates credentials → Creates ClaimsPrincipal
   → Sets role claim (e.g., "Mayor") → Cookie authentication → Dashboard
   ```

2. **Sidebar Rendering**:
   ```
   Page Load → _Sidebar.cshtml reads role from:
   ├─ Query parameter (?role=mayor) [for demo]
   └─ Claims Principal (ClaimTypes.Role) [for production]
   → Conditional rendering based on role → Show/hide menu items
   ```

3. **Controller Protection**:
   ```
   Request → Controller action → BaseController.GetCurrentUser()
   → Validates role → Allow/Deny access → Return view or redirect
   ```

### Multi-Layer Security:
- ✅ Sidebar visibility (UI layer)
- ✅ Controller authorization (Business logic layer)
- ✅ Service layer validation (Data access layer)
- ✅ Audit trail logging (All actions logged)

---

## 📱 Responsive Behavior

All sidebar modules are **fully responsive**:

### Desktop (≥1024px):
- Sidebar always visible on left
- Full module names shown
- Icons + text labels

### Tablet (768px - 1023px):
- Collapsible sidebar
- Hamburger menu toggle
- Icons + text labels

### Mobile (<768px):
- Slide-out sidebar overlay
- Touch-optimized
- Icons + text labels
- Closes after navigation

---

## 🚀 Implementation Status

### ✅ Completed:
- [x] Role detection from claims (bug fixed)
- [x] Mayor modules (7 items)
- [x] Executive Admin modules (9 items)
- [x] Records Officer modules (5 items)
- [x] Oversight Officer modules (7 items)
- [x] Conditional rendering based on role
- [x] Active state highlighting
- [x] Responsive design
- [x] SVG icons for all modules

### 📋 To Test:
- [ ] Login as each role and verify menu items
- [ ] Test module access permissions
- [ ] Verify exclusive modules are truly exclusive
- [ ] Test on mobile devices
- [ ] Check active state on each page

---

## 🔧 Customization Guide

### To Add a New Module:

1. **Add to Sidebar** (`Views/Shared/_Sidebar.cshtml`):
```razor
@if (role == TrackNGo.Models.UserRole.YourRole)
{
    <a href="/YourController/Index?role=@roleParam" 
       class="sidebar__link @(currentController == "yourcontroller" ? "sidebar__link--active" : "")" 
       id="sidebarYourModule">
        <svg viewBox="0 0 24 24"><!-- Your icon SVG --></svg>
        <span>Your Module Name</span>
    </a>
}
```

2. **Protect Controller**:
```csharp
public IActionResult Index()
{
    var user = GetCurrentUser();
    if (user == null || user.Role != UserRole.YourRole)
        return RedirectToAction("Index", "Dashboard");
    
    // Your logic here
}
```

3. **Update Documentation**:
- Add to ROLE_PROCESS_GUIDE.md
- Add to this SIDEBAR_MODULE_ACCESS.md
- Update role access matrix

---

## 📞 Support

For questions about role-based access:
- **System Administrator**: Check audit logs, verify role assignments
- **Developer**: Review `_Sidebar.cshtml`, `BaseController.cs`, `AuthService.cs`
- **User**: Contact your system administrator for role changes

---

**Last Updated**: June 28, 2026  
**Version**: 1.0  
**Based on**: ROLE_PROCESS_GUIDE.md
