# TrackNGo System - Complete Role Process Guide

## Table of Contents
1. [System Overview](#system-overview)
2. [User Roles & Access Matrix](#user-roles--access-matrix)
3. [Document Workflow Process](#document-workflow-process)
4. [Mayor Role Guide](#mayor-role-guide)
5. [Executive Admin Role Guide](#executive-admin-role-guide)
6. [Records Officer Role Guide](#records-officer-role-guide)
7. [CART/Oversight Officer Role Guide](#cartoversight-officer-role-guide)
8. [Public Client Access](#public-client-access)

---

## System Overview

**TrackNGo** is a Government Document Tracking System designed for Local Government Units (LGUs) with:
- **FSM-Based Workflow**: 11-state Finite State Machine for document lifecycle
- **ARTA Compliance**: Automated monitoring of processing periods (3/7/20 working days)
- **Role-Based Access Control**: 4 internal roles + public QR tracking
- **Digital Signatures**: Canvas-based Mayor signature with SHA-256 verification
- **Audit Trail**: Complete logging of all system actions
- **SMS Notifications**: Automated stakeholder updates at key workflow stages

### System Architecture
- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: Cookie-based session management
- **File Storage**: Local filesystem for attachments and QR codes

---

## User Roles & Access Matrix

### 1. Mayor (Local Chief Executive)
**UserRole.Mayor = 1**

| Module | Access Level | Key Actions |
|--------|--------------|-------------|
| Dashboard | Full Read | View pending signatures, ARTA flagged documents |
| Document View | Read Only | View all documents system-wide |
| Digital Signature | Exclusive Write | Sign/Approve/Reject documents |
| Workflow Routing | Full Access | View workflow dashboard, add comments |
| Comments | Write | Add remarks to documents |

**Cannot**: Create documents, Manage users, Export data, Resolve escalations

---

### 2. Executive Admin (Executive Assistants & Administrators)
**UserRole.ExecutiveAdmin = 2**

| Module | Access Level | Key Actions |
|--------|--------------|-------------|
| Dashboard | Full Read | View newly submitted, pending Mayor review |
| Document Management | Full CRUD | Create, endorse, forward, submit for approval |
| Workflow Routing | Full Access | Transition documents, add comments |
| User Administration | Full Access | Create users, reset passwords, view audit logs |
| System Configuration | Full Access | Manage departments, document types, workflow steps |
| Audit Trail | Read Only | View system audit logs |

**Cannot**: Sign documents, Export data, Resolve ARTA escalations

---

### 3. Records Officer (Records Officers & Receiving Clerks)
**UserRole.RecordsOfficer = 3**

| Module | Access Level | Key Actions |
|--------|--------------|-------------|
| Dashboard | Full Read | View received today, awaiting resubmission |
| Document Intake | Full Create | Register new documents, generate tracking numbers |
| Document List | Read Only | View documents, check statuses |
| Document Completion | Write | Mark documents as completed/released |
| QR Code Generation | Auto-Generate | Automatic on document creation |

**Cannot**: Endorse, Approve, Sign, Export data, Manage users, Access ARTA oversight

---

### 4. CART/Oversight Officer (Compliance, Accountability, Regulatory & Transparency)
**UserRole.OversightOfficer = 4**

| Module | Access Level | Key Actions |
|--------|--------------|-------------|
| Dashboard | Full Read | View warning/critical/overdue escalations |
| ARTA Oversight | Full Access | Monitor compliance, view escalation dashboard |
| Escalation Resolution | Exclusive Write | Resolve ARTA escalations |
| Report Generation | Full Access | Generate department performance, workflow, document reports |
| Data Export | Exclusive Write | Export CSV with secondary password |
| User Administration | Full Access | Create users, reset passwords, view audit logs |
| Audit Trail | Full Read | View all system audit logs |
| Notification History | Read Only | View SMS notification logs |

**Cannot**: Create documents, Sign documents, Transition workflow states

---

### 5. Public Client (No Login Required)
**No UserRole - QR Code Access Only**

| Module | Access Level | Key Actions |
|--------|--------------|-------------|
| Public Tracking | Read Only | Track document by reference number |
| Timeline View | Read Only | View document journey and current status |

**Cannot**: Login, Create/Modify documents, Access any administrative functions

---

## Document Workflow Process

### FSM Document States (11 States)

```
0. Submitted      → Document logged into system
1. Endorsed       → Classified and endorsed by Executive Admin
2. UnderReview    → Being reviewed by assigned personnel
3. ForApproval    → Forwarded to Mayor for signature
4. Approved       → Mayor has approved and digitally signed
5. Returned       → Sent back for revision (requires remarks)
6. Rejected       → Permanently rejected (terminal state)
7. ForRelease     → Approved and ready for release
8. Completed      → Released to client/stakeholder
9. Escalated      → Auto-escalated due to ARTA timeout
10. Forwarded     → Forwarded to another department
11. Accepted      → Accepted by target department
```

### ARTA Processing Periods

| Transaction Type | Processing Days | Usage |
|------------------|-----------------|-------|
| Simple | 3 working days | Routine requests, certifications |
| Complex | 7 working days | Permits requiring multi-department review |
| HighlyTechnical | 20 working days | Engineering plans, environmental studies |

**Warning Thresholds**:
- **Warning**: 75-99% of period used (yellow alert)
- **Critical**: Last working day before deadline (red alert)
- **Escalated**: Deadline exceeded (auto-escalation + document lock)

---

## Mayor Role Guide

### Role: Local Chief Executive
**System Access**: Mayor Dashboard, Document View, Digital Signature, Workflow Routing

### Responsibilities
- Review and approve/reject documents requiring executive action
- Digitally sign approved documents
- Monitor ARTA-flagged documents
- Add comments/instructions on documents

---

### Step-by-Step Process

#### 1. Login to System
**URL**: `/Auth/Login`
- Enter username and password
- System logs login in audit trail
- Redirected to Mayor Dashboard

#### 2. View Dashboard
**URL**: `/Dashboard/Index`

**Metrics Displayed**:
- **Pending Signatures**: Documents at ForApproval status awaiting Mayor's action
- **ARTA Flagged Documents**: Escalated documents requiring attention
- **Overall Compliance Rate**: System-wide ARTA compliance percentage
- **Recent Activity**: Last 10 system actions

**Actions Available**:
- Click "View Document" to review pending items
- Navigate to "Digital Signature" module
- View "Workflow Routing" dashboard

#### 3. Review Document for Approval
**URL**: `/Document/ViewDocument?id={documentId}`

**Information Displayed**:
- Tracking Number (TNG-YYYY-NNNN)
- Document Title & Type
- Originating Department
- Current Status (should be "ForApproval")
- Processing Timeline
- Workflow Transition History
- Previous Comments
- Attached Files
- ARTA Deadline Status

**Decision Points**:
- ✅ **Approve**: Document meets requirements → Proceed to Digital Signature
- ❌ **Reject**: Document has fundamental issues → Permanently reject
- 🔄 **Return**: Document needs revisions → Return with remarks

#### 4A. Approve Document (Digital Signature)
**URL**: `/Signature/Index`

**Process**:
1. Navigate to "Digital Signature" module (exclusive Mayor access)
2. Select document from "For Approval" queue
3. Review document details one final time
4. Draw signature on canvas using mouse/stylus
5. Select action: "Approved"
6. Click "Sign Document"

**System Actions**:
- Saves signature as PNG image
- Generates SHA-256 hash for integrity verification
- Stores signature record in DigitalSignatures table
- Transitions document from ForApproval → Approved
- Creates workflow transition record
- Logs action in audit trail
- Adds remark: "Signed by Local Chief Executive"
- Document moves to ForRelease queue

**Success Message**: "Document successfully signed and approved."

#### 4B. Reject Document
**URL**: `/Workflow/Transition` (POST)

**Process**:
1. From Document View page, click "Reject" button
2. Modal appears requesting remarks
3. Enter rejection reason (optional but recommended)
4. Confirm rejection

**System Actions**:
- Transitions document: ForApproval → Rejected
- Records rejection in workflow transitions
- Logs action in audit trail
- Document enters terminal state (no further processing)
- SMS notification sent to contact

**Use When**: Document violates policy, contains fraudulent info, or is fundamentally flawed

#### 4C. Return Document for Revision
**URL**: `/Workflow/Transition` (POST)

**Process**:
1. From Document View page, click "Return" button
2. **MANDATORY**: Enter detailed remarks explaining what needs correction
3. System validates remarks are not empty
4. Confirm return action

**System Actions**:
- Transitions document: ForApproval → Returned
- Stores remarks in workflow transition
- Logs action in audit trail
- SMS notification sent to contact: "Your document has been returned for revision"
- Records Officer can resubmit after corrections

**Use When**: Document needs corrections but is not fundamentally flawed

#### 5. Add Comments to Document
**URL**: `/Workflow/AddComment` (POST)

**Process**:
1. From Document View page, scroll to Comments section
2. Type comment in text area
3. Select comment type:
   - **Internal**: Visible only to system users
   - **Public**: Visible to clients via QR tracking
4. Click "Add Comment"

**System Actions**:
- Saves comment to DocumentComments table
- Associates comment with Mayor user ID
- Timestamps comment
- Comment appears in document timeline

**Use When**: Need to provide instructions, clarifications, or status updates

#### 6. View Workflow Routing Dashboard
**URL**: `/Workflow/Index`

**Information Displayed**:
- All active documents (not Completed or Rejected)
- Current status of each document
- Current office/department holding document
- Days elapsed vs ARTA period
- Document type and title

**Actions Available**:
- Click document to view details
- Monitor document flow across departments
- Identify bottlenecks

**Purpose**: High-level oversight of document processing across all departments

#### 7. Logout
**URL**: `/Auth/Logout`
- Click "Logout" in navigation
- Session terminated
- Logout logged in audit trail

---

### Mayor Module Access Summary

| Module | URL Path | Permission | Purpose |
|--------|----------|------------|---------|
| Dashboard | `/Dashboard/Index` | Read | View metrics and pending items |
| Document View | `/Document/ViewDocument` | Read | Review document details |
| Digital Signature | `/Signature/Index` | Write (Exclusive) | Sign/Approve/Reject documents |
| Workflow Routing | `/Workflow/Index` | Read | Monitor system-wide document flow |
| Workflow Transition | `/Workflow/Transition` | Write | Return documents for revision |
| Add Comment | `/Workflow/AddComment` | Write | Add remarks to documents |
| Notification History | `/Notification/Index` | Read | View SMS notifications |

---

## Executive Admin Role Guide

### Role: Executive Assistants & Administrators
**System Access**: Dashboard, Document Management, Workflow Routing, Administration

### Responsibilities
- Intake and register new documents
- Endorse and classify documents
- Route documents to appropriate departments
- Submit documents to Mayor for approval
- Manage users and system configuration
- Monitor workflow progress

---

### Step-by-Step Process

#### 1. Login to System
**URL**: `/Auth/Login`
- Enter username and password
- Redirected to Executive Admin Dashboard

#### 2. View Dashboard
**URL**: `/Dashboard/Index`

**Metrics Displayed**:
- **Newly Submitted**: Documents at Submitted status requiring endorsement
- **Pending Mayor Review**: Documents at ForApproval status
- **Total Documents**: System-wide document count
- **In Progress**: Active documents in workflow
- **ARTA Compliance Rate**: Overall compliance percentage

**Actions Available**:
- Navigate to "Document Management"
- Access "Administration" panel
- View "Workflow Routing" dashboard

#### 3. Intake New Document
**URL**: `/Document/Create`

**Process**:
1. Click "Intake New Document" button
2. **System Auto-Generates**: Tracking Number (TNG-YYYY-NNNN format)
3. Fill out document form:

**Required Fields**:
- **Title**: Document subject (max 300 chars)
- **Document Type**: Select from dropdown (Letter, Memorandum, Resolution, etc.)
- **Originating Department**: Department that created document
- **Submitted By**: Name of person submitting
- **Contact Number**: Mobile number for SMS notifications
- **Email Address**: Optional email contact
- **ARTA Period**: Select transaction type
  - Simple (3 days)
  - Complex (7 days)
  - HighlyTechnical (20 days)

**File Attachments**:
- Click "Choose Files" to select multiple attachments
- Supported formats: PDF, DOCX, XLSX, JPG, PNG
- Files stored in `/uploads/{TrackingNumber}/` folder

4. Click "Register Document"

**System Actions**:
- Creates Document record with status: Submitted
- Creates DocumentMetadata record
- Creates RoutingSlip record
- Generates QR Code with URL: `/Public/Track?refNumber={TrackingNumber}`
- Saves QR code to `/wwwroot/qrcodes/{TrackingNumber}.png`
- Creates QRCodeRecord entry
- Logs action in audit trail
- Sends SMS to contact: "Your document {TrackingNumber} has been received."

**Success Message**: "Document {TrackingNumber} successfully registered."

#### 4. Endorse Document
**URL**: `/Workflow/Transition` (POST)

**When**: After reviewing newly submitted document

**Process**:
1. Navigate to "Document List" (`/Document/Index`)
2. Filter by Status: "Submitted"
3. Click document to view details
4. Click "Endorse" button
5. Select action: "Endorse"
6. Optionally add remarks
7. Submit

**System Actions**:
- Transitions: Submitted → Endorsed
- Records transition in WorkflowTransitions
- Updates document CurrentStatus
- Logs in audit trail

**Purpose**: Classify document and confirm it's ready for processing

#### 5. Forward Document to Department
**URL**: `/Workflow/Transition` (POST)

**When**: Document needs review by specific department

**Process**:
1. View document details
2. Click "Forward" button
3. Select "To Office" from dropdown (Engineering, Legal, Finance, etc.)
4. Add forwarding instructions in remarks
5. Submit

**System Actions**:
- Transitions: Current → Forwarded
- Records ToOffice in workflow transition
- Updates CurrentOfficeName on document
- Sends SMS if department head has mobile number
- Logs in audit trail

**Purpose**: Route document to appropriate department for review

#### 6. Submit Document to Mayor for Approval
**URL**: `/Workflow/Transition` (POST)

**When**: Document has been reviewed and is ready for Mayor's signature

**Process**:
1. View document details
2. Verify all required attachments are present
3. Verify all department reviews are complete
4. Click "Submit for Approval" button
5. Add summary remarks for Mayor
6. Confirm submission

**System Actions**:
- Transitions: Current → ForApproval
- Sets ToOffice: "Office of the Mayor"
- Updates CurrentOfficeName: "Mayor's Office"
- Document appears in Mayor's "Pending Signatures" queue
- Logs in audit trail

**Important**: Once submitted, only Mayor can approve, reject, or return the document

#### 7. Accept Returned Document
**URL**: `/Workflow/Transition` (POST)

**When**: Mayor returns document for revision

**Process**:
1. View returned document (Status: Returned)
2. Read Mayor's remarks explaining required corrections
3. Make necessary corrections to attachments
4. Click "Resubmit" button
5. Add note explaining corrections made
6. Confirm resubmission

**System Actions**:
- Transitions: Returned → Submitted
- Document re-enters workflow
- Can follow same process: Endorse → Review → Submit for Approval

#### 8. Manage Users
**URL**: `/Admin/CreateUser`

**Process**:
1. Navigate to "Administration" panel (`/Admin/Index`)
2. Click "Create New User"
3. Fill out user form:

**Required Fields**:
- Username (unique, 3-50 characters)
- Full Name
- Password (min 6 characters)
- Role: Select from
  - Mayor
  - Executive Admin
  - Records Officer
  - Oversight Officer
- Department
- Email (optional)
- Phone Number (optional)
- Mobile Number (for SMS)

4. Click "Create User"

**System Actions**:
- Hashes password using SHA-256
- Creates User record with IsActive = true
- Logs user creation in audit trail
- New user can immediately login

**Purpose**: Onboard new staff members to system

#### 9. Reset User Password
**URL**: `/Admin/ResetPassword` (POST)

**Process**:
1. From Administration panel, view user list
2. Find user needing password reset
3. Click "Reset Password"
4. Enter new password
5. Confirm reset

**System Actions**:
- Hashes new password
- Updates user's PasswordHash
- Logs password reset in audit trail
- User can login with new password

#### 10. View Audit Logs
**URL**: `/Admin/Audit`

**Information Displayed**:
- Action timestamp
- Document tracking number (if applicable)
- User who performed action
- Action type (DocumentCreated, StatusChanged, UserLogin, etc.)
- Description/details
- IP address

**Filters Available**:
- Date range
- User
- Action type
- Document

**Purpose**: Monitor system activity, investigate issues, compliance reporting

#### 11. Configure System Settings
**URL**: `/Admin/Index`

**Manage**:
- **Departments**: Add/edit department names and codes
- **Document Types**: Configure document categories and workflow steps
- **Workflow Steps**: Define FSM states for each document type
- **ARTA Periods**: Set processing periods for transaction types

---

### Executive Admin Module Access Summary

| Module | URL Path | Permission | Purpose |
|--------|----------|------------|---------|
| Dashboard | `/Dashboard/Index` | Read | View metrics |
| Document Intake | `/Document/Create` | Create | Register new documents |
| Document List | `/Document/Index` | Read | View all documents |
| Document View | `/Document/ViewDocument` | Read | Review details |
| Workflow Routing | `/Workflow/Index` | Full | Monitor and route documents |
| Workflow Transition | `/Workflow/Transition` | Write | Endorse, forward, submit |
| Add Comment | `/Workflow/AddComment` | Write | Add remarks |
| User Management | `/Admin/CreateUser` | Full | Create/edit users |
| Administration | `/Admin/Index` | Full | System configuration |
| Audit Trail | `/Admin/Audit` | Read | View system logs |
| Notification History | `/Notification/Index` | Read | View SMS logs |

---

## Records Officer Role Guide

### Role: Records Officers & Receiving Clerks
**System Access**: Dashboard, Document Intake, Document List, Document Completion

### Responsibilities
- Receive and register incoming documents
- Generate tracking numbers and QR codes
- Complete and release documents to clients
- Handle document resubmissions after corrections
- Monitor received documents

---

### Step-by-Step Process

#### 1. Login to System
**URL**: `/Auth/Login`
- Enter username and password
- Redirected to Records Officer Dashboard

#### 2. View Dashboard
**URL**: `/Dashboard/Index`

**Metrics Displayed**:
- **Received Today**: Documents registered today
- **Awaiting Resubmission**: Documents with Returned status
- **Total Documents**: System-wide count
- **In Progress**: Active documents
- **Completed Count**: Released documents

**Actions Available**:
- Navigate to "Intake New Document"
- View "Awaiting Resubmission" queue
- Access "Document List"

#### 3. Receive Walk-In Document
**URL**: `/Document/Create`

**Physical Process**:
1. Client arrives at Records Office with document
2. Records Officer receives physical document
3. Verifies completeness of attachments
4. Stamps "RECEIVED" with date

**Digital Process**:
1. Click "Intake New Document"
2. System displays auto-generated Tracking Number: **TNG-2026-0312**
3. Fill out intake form:

**Required Information**:
- **Title**: What client calls the document (e.g., "Business Permit Application")
- **Document Type**: Select category (Application, Letter, Request, etc.)
- **Originating Department**: Client's department OR external if from citizen
- **Submitted By**: Client's full name
- **Contact Number**: Client's mobile (for SMS updates) - **IMPORTANT**
- **Email**: Optional
- **ARTA Period**: 
  - Ask client or refer to service charter
  - Simple = 3 days, Complex = 7 days, HighlyTechnical = 20 days

**Upload Files**:
- Scan physical attachments to PDF
- Click "Choose Files" button
- Select scanned files
- Multiple files allowed

4. Click "Register Document"

**System Actions**:
- Document created with status: Submitted
- Tracking number permanently assigned
- QR code generated with tracking URL
- QR code image saved: `/wwwroot/qrcodes/TNG-2026-0312.png`
- Routing slip created
- SMS sent to client: "Your document TNG-2026-0312 has been received. Track at: [URL]"
- Audit log entry created

5. **Physical Actions**:
- Print QR code from system
- Attach QR code sticker to physical document
- Print routing slip
- Attach routing slip to document folder
- File physical document in Records Office
- Give client a copy of tracking number

**Success Message**: "Document TNG-2026-0312 successfully registered."

#### 4. Track Document Progress
**URL**: `/Tracking/Index` or `/Document/Index`

**Process**:
1. Navigate to "Document List"
2. Filter by tracking number, date, or status
3. View document details
4. Check current office holding document
5. View processing timeline

**Information Visible**:
- Current status
- Current office/department
- Days elapsed
- ARTA deadline
- Workflow transition history
- Comments

**Purpose**: Answer client inquiries about document status

#### 5. Handle Returned Documents
**URL**: `/Document/ViewDocument?id={docId}`

**When**: Document returned by Mayor or Executive Admin for corrections

**Process**:
1. Check "Awaiting Resubmission" queue on dashboard
2. Click document to view details
3. Read remarks explaining required corrections
4. Contact client to request corrections
5. Client submits corrected version
6. Upload new files replacing old ones
7. Click "Resubmit" button
8. Add note: "Corrections made as per remarks"

**System Actions**:
- Transitions: Returned → Submitted
- Document re-enters workflow
- SMS sent to client: "Your document has been resubmitted."
- Workflow history preserved (return reason visible)

#### 6. Complete and Release Document
**URL**: `/Workflow/Transition` (POST)

**When**: Document has been approved by Mayor and is ForRelease status

**Process**:
1. View document with status: ForRelease
2. Verify Mayor's signature is present
3. Prepare physical document for release
4. Contact client for pickup
5. Client arrives to claim document
6. Verify client identity
7. Click "Complete" button in system
8. Add note: "Released to {client name} on {date}"
9. Confirm completion

**System Actions**:
- Transitions: ForRelease → Completed
- Sets DateCompleted to current timestamp
- SMS sent to client: "Your document is ready for pickup."
- Document appears in "Completed" reports
- Removes from active workflow queue

**Physical Actions**:
- Client signs logbook acknowledging receipt
- Give client original signed document
- Retain file copies for archives

#### 7. Generate Document Reports
**URL**: `/Document/Index` with filters

**Process**:
1. Navigate to Document List
2. Apply filters:
   - Date Range: "Received from [date] to [date]"
   - Status: All, Completed, In Progress
   - Document Type
   - Department
3. View results
4. Export to PDF/print if needed

**Use Cases**:
- Daily receipt reports
- Monthly completion statistics
- ARTA compliance tracking

---

### Records Officer Module Access Summary

| Module | URL Path | Permission | Purpose |
|--------|----------|------------|---------|
| Dashboard | `/Dashboard/Index` | Read | View received today, awaiting resubmission |
| Document Intake | `/Document/Create` | Create | Register new documents |
| Document List | `/Document/Index` | Read | View documents |
| Document View | `/Document/ViewDocument` | Read | Check document details |
| Document Tracking | `/Tracking/Index` | Read | Track document progress |
| Document Completion | `/Workflow/Transition` | Write (Complete only) | Mark as completed |
| Resubmit Documents | `/Workflow/Transition` | Write (Resubmit only) | Handle returned docs |

**Cannot Access**:
- Digital Signature module
- User administration
- ARTA oversight module
- Data export
- System configuration

---

## CART/Oversight Officer Role Guide

### Role: Compliance, Accountability, Regulatory & Transparency Personnel
**System Access**: ARTA Oversight, Reports, Data Export, Administration (User Management)

### Responsibilities
- Monitor ARTA compliance (3/7/20-day periods)
- Resolve escalated documents
- Generate compliance reports
- Export system data for audits
- Create users and view audit trails
- Flag non-compliant processes

---

### Step-by-Step Process

#### 1. Login to System
**URL**: `/Auth/Login`
- Enter username and password
- Redirected to ARTA Oversight Dashboard

#### 2. View ARTA Oversight Dashboard
**URL**: `/Oversight/Index`

**Key Metrics Displayed**:

| Metric | Description | Threshold |
|--------|-------------|-----------|
| **Total Active** | Documents currently in workflow | N/A |
| **On Track** | Documents within 75% of ARTA period | Green |
| **Warning** | Documents using 75-99% of period | Yellow |
| **Critical** | Documents on last day before deadline | Orange |
| **Overdue** | Documents past ARTA deadline | Red |
| **Escalated Count** | Auto-escalated documents | Alert |
| **Overall Compliance Rate** | % of documents completed within ARTA period | Target: 100% |

**Visual Indicators**:
- **Green**: 0-74% of period used
- **Yellow**: 75-99% of period used
- **Orange**: Last working day
- **Red**: Past deadline (escalated)

**Document Lists**:
- **Escalated Documents Table**: Shows overdue documents
  - Tracking Number
  - Title
  - Days Overdue
  - Current Office (where bottleneck is)
  - ARTA Period
  - Escalation Level
  - Action button: "Resolve"

- **Warning Documents Table**: Shows at-risk documents
  - Tracking Number
  - Days Elapsed / ARTA Period
  - Percentage Used
  - Current Office

#### 3. Monitor ARTA Compliance
**URL**: `/Oversight/Index`

**Daily Monitoring Routine**:

1. **Check Warning Count** (Yellow Alert)
   - Documents at 75-99% of ARTA period
   - Action: Contact holding office, request priority processing
   - No system action needed yet

2. **Check Critical Count** (Orange Alert)
   - Documents on last working day before deadline
   - Action: Escalate to department head, request immediate processing
   - System will auto-escalate tomorrow if not completed

3. **Check Overdue Count** (Red Alert)
   - Documents past ARTA deadline
   - Status automatically changed to: Escalated
   - Document is locked from normal workflow
   - Action: Resolve escalation

**ARTA Auto-Escalation Process** (Automatic - No User Action):
- System background job runs daily at 12:00 AM
- Calculates working days elapsed for each active document
- Compares to ARTA period (3, 7, or 20 days)
- If `DaysElapsed >= ARTAProcessingDays`:
  - Status: Current → Escalated
  - IsEscalated: true
  - IsLocked: true (no further transitions allowed)
  - EscalationLog entry created
  - SMS sent to all stakeholders
  - Oversight Officer notified

#### 4. Resolve Escalation
**URL**: `/Oversight/Resolve` (POST)

**When**: Document has been escalated due to ARTA timeout

**Process**:
1. From Oversight Dashboard, locate escalated document
2. Click "Resolve" button
3. Investigation required:
   - Why did delay occur?
   - Which office caused bottleneck?
   - Was delay justifiable?
   - What corrective action was taken?

4. Enter **Resolution Notes** (MANDATORY):
   - Example: "Delay caused by missing attachment. Client provided corrected document on [date]. Document expedited for Mayor approval."
   - Include: Root cause, corrective action, persons responsible

5. Click "Submit Resolution"

**System Actions**:
- Creates EscalationLog entry with resolution
- Sets EscalationLog.ResolvedByUserId = Oversight Officer ID
- Sets EscalationLog.ResolutionDate = current timestamp
- IsLocked: true → false (unlocks document)
- Status remains: Escalated (for reporting)
- Document can now continue workflow
- Logs resolution in audit trail

**Important**: Only Oversight Officer can resolve escalations (exclusive authority)

#### 5. Generate Compliance Reports
**URL**: `/Oversight/Reports`

**Report Types Available**:

##### A. Department Performance Report
**Purpose**: Track which departments meet ARTA deadlines

**Process**:
1. Navigate to Reports section
2. Select "Department Performance"
3. Set date range (e.g., "January 2026 - March 2026")
4. Click "Generate Report"

**Report Contents**:
- Department name
- Total documents processed
- Documents completed on time
- Documents escalated
- Average processing time
- Compliance rate (%)
- Ranking by compliance

**Use Case**: Identify high-performing and underperforming departments

##### B. Workflow Analysis Report
**Purpose**: Identify bottlenecks in document flow

**Process**:
1. Select "Workflow Analysis"
2. Set parameters:
   - Document Type: All, Letter, Memorandum, Resolution
   - Date Range
3. Click "Generate"

**Report Contents**:
- Average days per workflow stage:
  - Submitted → Endorsed
  - Endorsed → UnderReview
  - UnderReview → ForApproval
  - ForApproval → Approved
  - Approved → Completed
- Bottleneck identification (longest average stage)
- Total workflow duration

**Use Case**: Process improvement initiatives

##### C. Document Summary Report
**Purpose**: Executive summary of document statistics

**Process**:
1. Select "Document Summary"
2. Set date range
3. Click "Generate"

**Report Contents**:
- Total documents received
- Total completed
- Total in progress
- Total returned
- Total rejected
- Total escalated
- Overall completion rate
- Overall ARTA compliance rate

**Use Case**: Monthly/quarterly reporting to Mayor

#### 6. Export System Data
**URL**: `/Export/ExportData` (POST)

**SECURITY**: Requires secondary password authentication

**Process**:
1. Navigate to "Export Data" section
2. Set export filters:
   - **Document Type**: All, or specific type
   - **Department**: All, or specific department
   - **Status**: All, Completed, In Progress, Escalated, etc.
   - **Date Range**: From [date] To [date]

3. Enter **Export Password**:
   - This is DIFFERENT from login password
   - Configured by system administrator
   - Stored separately for audit security
   - Example: "CART-Export-2026"

4. Click "Export to CSV"

**System Validation**:
- Validates export password against stored hash
- If incorrect: 
  - Error message: "Invalid export password. Access denied."
  - Failed attempt logged in ExportAuditLog
  - Status: "Failed"
- If correct:
  - Proceeds with export

**Export File Contents** (CSV format):
```
TrackingNumber,Title,DocumentType,Department,SubmittedBy,DateFiled,
CurrentStatus,CurrentOffice,DaysElapsed,ARTAPeriod,IsEscalated,DateCompleted
TNG-2026-0001,"Business Permit","Application","Engineering","Juan Dela Cruz",
"2026-01-15","Completed","Records Office",5,7,FALSE,"2026-01-20"
...
```

**System Actions**:
- Generates CSV file with filtered data
- Logs export in ExportAuditLog:
  - ExportedByUserId
  - ExportType: "CSV"
  - Scope: Filter parameters
  - DataSize: File size in bytes
  - Status: "Success"
  - Timestamp
  - IP Address
- Returns file download: `TrackNGo_Export_20260628.csv`

**Use Cases**:
- Annual audit submissions
- Commission on Audit (COA) compliance
- ARTA reporting to national government
- Data backup
- External analysis (Excel pivot tables, Power BI)

**Security Note**: All export attempts (success and failure) are permanently logged

#### 7. Create System Users
**URL**: `/Admin/CreateUser`

**Process**: (Same as Executive Admin - see above)
- Navigate to Administration panel
- Click "Create New User"
- Fill out user form
- Assign role and department
- Submit

**Note**: Oversight Officer has same user management permissions as Executive Admin

#### 8. View Audit Trail
**URL**: `/Admin/Audit`

**Process**:
1. Navigate to "Audit Trail" from Administration menu
2. View comprehensive log of all system actions
3. Filter by:
   - Date range
   - User
   - Action type
   - Document

**Audit Entry Information**:
- Timestamp (date and time)
- User who performed action
- Document affected (tracking number)
- Action type:
  - UserLogin / UserLogout
  - DocumentCreated
  - StatusChanged
  - UserCreated
  - PasswordReset
  - SignatureApplied
  - EscalationResolved
  - DataExported
- Description/details
- IP address

**Use Cases**:
- Investigate security incidents
- Track user activity
- Compliance audits
- Dispute resolution
- Performance monitoring

---

### CART/Oversight Officer Module Access Summary

| Module | URL Path | Permission | Purpose |
|--------|----------|------------|---------|
| Dashboard | `/Dashboard/Index` | Read | View escalation metrics |
| ARTA Oversight | `/Oversight/Index` | Full | Monitor compliance, view warnings |
| Resolve Escalation | `/Oversight/Resolve` | Write (Exclusive) | Unlock escalated documents |
| Generate Reports | `/Oversight/Reports` | Full | Department, workflow, summary reports |
| Export Data | `/Export/ExportData` | Write (Exclusive) | CSV export with password |
| User Management | `/Admin/CreateUser` | Full | Create users, reset passwords |
| Audit Trail | `/Admin/Audit` | Read | View system logs |
| Notification History | `/Notification/Index` | Read | View SMS logs |

**Cannot Access**:
- Document intake/creation
- Digital signature module
- Workflow transitions (endorse, forward, approve)
- Document editing

---

## Public Client Access

### Role: External Clients/Citizens (No Login)
**System Access**: Public Tracking via QR Code

### Process

#### 1. Receive Tracking Number
**When**: After submitting document at Records Office

**What Client Receives**:
- Tracking Number: **TNG-2026-0312**
- QR Code sticker (attached to physical document)
- SMS notification: "Your document TNG-2026-0312 has been received. Track at: http://yourLGU.gov.ph/Public/Track?refNumber=TNG-2026-0312"

#### 2. Track Document Using QR Code
**Method A: Scan QR Code**

**Process**:
1. Open phone camera app
2. Point camera at QR code on document
3. Tap notification that appears
4. Browser opens to tracking page

**Method B: Click SMS Link**

**Process**:
1. Open SMS from TrackNGo
2. Tap tracking link in message
3. Browser opens to tracking page

**Method C: Manual Entry**

**Process**:
1. Visit LGU website
2. Navigate to "Track Document" section
3. Enter tracking number: TNG-2026-0312
4. Click "Track"

#### 3. View Document Status
**URL**: `/Public/Track?refNumber=TNG-2026-0312`

**Information Displayed**:

**Document Overview**:
- Tracking Number: TNG-2026-0312
- Title: Business Permit Application
- Document Type: Application
- Date Filed: January 15, 2026
- Current Status: **Under Review** ✓
- Current Office: Engineering Office
- Days Elapsed: 3 of 7 (ARTA: Complex)
- Progress Bar: 43% ■■■■□□□□□□

**Timeline/Journey** (Visual stepper):
```
✓ Submitted         Jan 15, 2026, 9:30 AM
  └─ Received by Records Office

✓ Endorsed          Jan 15, 2026, 10:15 AM
  └─ Classified and endorsed by Admin

✓ Forwarded         Jan 16, 2026, 2:00 PM
  └─ Sent to Engineering Office for review

● Under Review      Jan 17, 2026, 8:45 AM
  └─ Engineering is currently reviewing

○ For Approval      (Pending)
  └─ Will be sent to Mayor after review

○ Approved          (Pending)
  └─ Awaiting Mayor's signature

○ Completed         (Pending)
  └─ Ready for release
```

**Public Comments** (if any):
- Jan 17: "Site inspection scheduled for Jan 20"
- Jan 16: "Please prepare updated floor plan"

**Scan Count**: This document has been tracked 5 times

#### 4. Understanding Status Messages

| Status Display | Meaning | What Client Should Do |
|----------------|---------|----------------------|
| **Submitted** | Document received by Records Office | Wait for endorsement |
| **Endorsed** | Document classified and ready for processing | No action needed |
| **Under Review** | Being reviewed by department | Wait for review completion |
| **Forwarded** | Sent to another department | Wait for department to accept |
| **For Approval** | With Mayor for signature | Wait for Mayor's action |
| **Approved** | Mayor has signed | Document processing complete |
| **For Release** | Ready for pickup | Visit Records Office to claim |
| **Completed** | Released to client | Document journey finished |
| **Returned** | Needs corrections | Check remarks, submit corrected version |
| **Rejected** | Not approved | See rejection reason, file new request if needed |

#### 5. Receive SMS Notifications

**Automatic Notifications Sent**:

1. **Document Received**
   - When: Document first registered
   - Message: "Your document TNG-2026-0312 has been received. Track at [URL]"

2. **Status Update**
   - When: Document moves to new status
   - Message: "Update: Your document TNG-2026-0312 is now [Status]. Current office: [Office Name]"

3. **Returned for Revision**
   - When: Mayor or Admin returns document
   - Message: "Your document TNG-2026-0312 has been returned. Reason: [Remarks]. Please resubmit corrected version."

4. **Approved and Ready**
   - When: Document approved by Mayor
   - Message: "Good news! Your document TNG-2026-0312 has been approved and is ready for release."

5. **Escalation Alert**
   - When: Document exceeds ARTA deadline
   - Message: "Alert: Your document TNG-2026-0312 has been escalated. We're working to resolve the delay."

**SMS Features**:
- Stored in SMSNotification table
- Delivery status tracked: Queued → Sent → Delivered/Failed
- Client mobile number from document intake form

---

### Public Client Features Summary

| Feature | Access | Purpose |
|---------|--------|---------|
| QR Code Tracking | No login required | Check document status anytime |
| Timeline View | Public URL | See document journey |
| Status Updates | SMS notifications | Receive automatic alerts |
| Public Comments | Read-only | View stakeholder remarks |

**Cannot Do**:
- Login to system
- Modify documents
- View internal comments
- Access other clients' documents
- View system statistics

---

## Complete Workflow Scenarios

### Scenario 1: Standard Approval Flow

**Document**: Business Permit Application (Complex - 7 days)

**Day 1 - 9:00 AM**: Client submits at Records Office
- **Records Officer**: Creates document, uploads scanned files
- System: Generates TNG-2026-0312, QR code, sends SMS
- Status: **Submitted**

**Day 1 - 10:00 AM**: Executive Admin reviews
- **Executive Admin**: Reviews submission, clicks "Endorse"
- Status: **Submitted** → **Endorsed**

**Day 1 - 2:00 PM**: Forward to Engineering
- **Executive Admin**: Clicks "Forward", selects "Engineering Office"
- Status: **Endorsed** → **Forwarded**
- SMS sent to Engineering head

**Day 2 - 8:00 AM**: Engineering accepts
- **Engineering Staff**: Reviews document, conducts site inspection
- Status: **Forwarded** → **Under Review**

**Day 3 - 3:00 PM**: Engineering completes review
- **Executive Admin**: Receives Engineering clearance, adds comment
- Status: **Under Review** → **Endorsed**

**Day 4 - 9:00 AM**: Submit to Mayor
- **Executive Admin**: Clicks "Submit for Approval"
- Status: **Endorsed** → **For Approval**
- Document appears in Mayor's queue

**Day 5 - 10:00 AM**: Mayor signs
- **Mayor**: Reviews document, opens Digital Signature module
- **Mayor**: Draws signature, clicks "Approve"
- Status: **For Approval** → **Approved**
- SMS sent to client: "Your document has been approved"

**Day 5 - 11:00 AM**: Prepare for release
- **Executive Admin**: Clicks "Release"
- Status: **Approved** → **For Release**

**Day 6 - 9:00 AM**: Client picks up
- **Records Officer**: Client arrives, verifies identity
- **Records Officer**: Clicks "Complete"
- Status: **For Release** → **Completed**
- Total Processing Time: **5 working days** (within 7-day ARTA period) ✓
- SMS sent: "Your document is ready for pickup"

---

### Scenario 2: Document Return Flow

**Document**: Zoning Clearance (Simple - 3 days)

**Day 1**: Document submitted and endorsed (same as Scenario 1)

**Day 2**: Submitted to Mayor
- Status: **For Approval**

**Day 2 - 3:00 PM**: Mayor reviews, finds issue
- **Mayor**: Notices missing attachment
- **Mayor**: Clicks "Return" button
- **Mayor**: Enters remarks: "Site development plan is missing. Please attach updated plan showing compliance with setback requirements."
- Status: **For Approval** → **Returned**
- SMS sent to client with remarks

**Day 3**: Client submits corrected version
- **Records Officer**: Client brings missing attachment
- **Records Officer**: Views returned document
- **Records Officer**: Uploads new file
- **Records Officer**: Clicks "Resubmit"
- Status: **Returned** → **Submitted**
- Document re-enters workflow

**Day 4**: Fast-tracked re-processing
- **Executive Admin**: Endorses immediately
- **Executive Admin**: Submits to Mayor again
- Status: **Submitted** → **Endorsed** → **For Approval**

**Day 4 - 2:00 PM**: Mayor approves
- **Mayor**: Verifies correction, signs
- Status: **For Approval** → **Approved** → **For Release** → **Completed**
- Total time including return: **4 days** (exceeded 3-day ARTA) ⚠️
- But justifiable due to client error

---

### Scenario 3: ARTA Escalation & Resolution

**Document**: Certificate of No Pending Case (Simple - 3 days)

**Day 1 - 9:00 AM**: Document submitted
- Status: **Submitted**
- ARTA Deadline: Day 4 (3 working days)

**Day 1 - 11:00 AM**: Endorsed
- Status: **Endorsed**

**Day 2 - 8:00 AM**: Forwarded to Legal Office
- Status: **Forwarded**

**Day 3 - 9:00 AM**: Still in Legal Office
- **System**: Warning threshold reached (75%)
- **Oversight Dashboard**: Shows document in "Warning" list (yellow)
- **Oversight Officer**: Calls Legal Office, requests priority

**Day 3 - 5:00 PM**: No progress
- **System**: Critical threshold (90%)
- **Oversight Dashboard**: Moves to "Critical" list (orange)

**Day 4 - 11:59 PM**: System auto-escalation
- **System Background Job**: Runs at midnight
- **System**: Calculates: Elapsed = 3 days, ARTA Period = 3 days
- **System**: DEADLINE EXCEEDED
- **System Actions**:
  - Status: **Forwarded** → **Escalated**
  - IsEscalated: true
  - IsLocked: true
  - EscalationLog created:
    - Level: "Overdue"
    - ResponsibleOffice: "Legal Office"
    - DaysOverdue: 1
  - SMS sent to all stakeholders: "ALERT: Document TNG-2026-0567 has exceeded ARTA deadline"
  - Oversight Officer notified

**Day 5 - 8:00 AM**: Oversight Officer investigates
- **Oversight Officer**: Views escalation dashboard
- **Oversight Officer**: Sees TNG-2026-0567 in red
- **Oversight Officer**: Contacts Legal Office head
- **Finding**: Assigned staff was on sick leave, document not reassigned

**Day 5 - 10:00 AM**: Legal completes review
- **Legal Office**: Provides clearance to Executive Admin
- **Executive Admin**: Cannot transition (document is locked)
- **Executive Admin**: Contacts Oversight Officer

**Day 5 - 11:00 AM**: Oversight resolves escalation
- **Oversight Officer**: Clicks "Resolve" button
- **Oversight Officer**: Enters resolution notes:
  ```
  Root Cause: Staff absence without delegation
  Corrective Action: Implemented sick leave backup protocol
  Resolution: Document cleared by Legal as of Day 5
  Recommendation: Review workload distribution in Legal Office
  ```
- **System Actions**:
  - IsLocked: true → false
  - EscalationLog updated with resolution
  - Document can now be transitioned
  - Status remains: **Escalated** (for permanent record)

**Day 5 - 2:00 PM**: Continue workflow
- **Executive Admin**: Submits to Mayor
- Status: **Escalated** → **For Approval**
- **Mayor**: Signs same day
- Status: **For Approval** → **Approved** → **Completed**
- **Final Processing Time**: 5 days (2 days over ARTA)
- **Compliance**: Failed, but justified and documented

---

## System Security & Compliance

### Authentication & Authorization

**Login Security**:
- Cookie-based session management
- Password hashing: SHA-256 (demo) / PBKDF2 (production recommended)
- Session timeout: 30 minutes inactivity
- All login/logout events logged in audit trail

**Role-Based Access Control (RBAC)**:
- Strict controller-level authorization checks
- Each action validates user role before executing
- Unauthorized access redirects to Dashboard
- Exclusive permissions enforced:
  - **Digital Signature**: Mayor ONLY
  - **Export Data**: Oversight Officer ONLY
  - **Resolve Escalations**: Oversight Officer ONLY

### Audit Trail Logging

**All Actions Logged**:
- User authentication (login/logout)
- Document creation
- Status transitions
- Signature applications
- User management (create/edit/password reset)
- Data exports (success and failures)
- Escalation resolutions
- System configuration changes

**Audit Entry Contains**:
- Timestamp (UTC)
- User ID and name
- Document ID and tracking number
- Action type
- Description/details
- IP address
- Result (success/failure)

**Purpose**:
- Compliance audits (COA, ARTA, internal)
- Security incident investigation
- Performance monitoring
- Dispute resolution
- Accountability tracking

### Data Export Security

**Secondary Password Gate**:
- Export function requires separate password
- Different from login password
- Stored as hashed value
- Prevents unauthorized data extraction
- All export attempts logged

**Export Audit Log**:
- Who exported
- What data (filters applied)
- When (timestamp)
- Data size (bytes)
- Success/failure status
- IP address

### ARTA Compliance Features

**Automated Monitoring**:
- Background job runs daily at midnight
- Calculates working days (excludes weekends, holidays)
- Identifies documents at 75%, 90%, 100%+ of ARTA period
- Auto-escalates overdue documents
- No human intervention required

**Compliance Reporting**:
- Department performance metrics
- Overall compliance rate calculation
- Escalation trend analysis
- Bottleneck identification

**Document Locking**:
- Escalated documents automatically locked
- Prevents further status changes until resolved
- Ensures escalation is formally addressed
- Only Oversight Officer can unlock

---

## Common Troubleshooting

### For All Users

**Problem**: Cannot login
- **Check**: Username and password are correct (case-sensitive)
- **Check**: Account is active (IsActive = true)
- **Solution**: Contact Administrator for password reset

**Problem**: Cannot see document in list
- **Check**: Apply correct filters (status, date range, department)
- **Check**: Role permissions (some documents visible only to certain roles)
- **Solution**: Try "Clear Filters" or contact Administrator

**Problem**: Document not moving to next status
- **Check**: Document is not locked (IsLocked = false)
- **Check**: Current status allows desired transition
- **Check**: User role has permission for this action
- **Solution**: Check workflow transition rules or contact Oversight

---

### For Mayor

**Problem**: Cannot access Digital Signature module
- **Check**: You are logged in as Mayor role
- **Solution**: Only Mayor role has signature access

**Problem**: No documents in "For Approval" queue
- **Check**: Executive Admin must submit documents via "Submit for Approval"
- **Check**: Filter by status: "For Approval"

**Problem**: Signature won't save
- **Check**: Canvas has signature drawing (not blank)
- **Check**: Document is in ForApproval status
- **Solution**: Try redrawing signature, ensure browser supports HTML5 canvas

---

### For Executive Admin

**Problem**: Cannot endorse submitted document
- **Check**: Document status is "Submitted"
- **Check**: No validation errors in document record
- **Solution**: View document details to identify issues

**Problem**: "Submit for Approval" button disabled
- **Check**: Document has completed all required reviews
- **Check**: All required attachments are present
- **Recommendation**: Add comment explaining approval readiness

**Problem**: Cannot create new user
- **Check**: Username is unique (not already taken)
- **Check**: All required fields completed
- **Check**: Password meets minimum length (6 characters)

---

### For Records Officer

**Problem**: QR code not generating
- **Check**: Tracking number was properly generated
- **Check**: Web server has write permission to /wwwroot/qrcodes/ folder
- **Solution**: Contact Administrator to check folder permissions

**Problem**: SMS not sent to client
- **Check**: Mobile number format is correct (e.g., +639171234567)
- **Check**: SMS service is configured in appsettings.json
- **Solution**: View Notification History to check delivery status

**Problem**: Cannot complete document
- **Check**: Document status is "For Release"
- **Check**: Mayor has already signed document
- **Solution**: Check workflow history to see current status

---

### For CART/Oversight Officer

**Problem**: Cannot resolve escalation
- **Check**: You are logged in as Oversight Officer role
- **Check**: Document is actually in Escalated status
- **Check**: Resolution notes are not empty
- **Solution**: Only Oversight Officer can resolve

**Problem**: Export fails with "Invalid password"
- **Check**: Using export password, not login password
- **Check**: Export password configured by Administrator
- **Solution**: Contact Administrator to verify/reset export password

**Problem**: Compliance rate calculation seems wrong
- **Check**: ARTA periods correctly set on documents
- **Check**: Working days calculation excludes weekends/holidays
- **Solution**: Review individual document timelines for accuracy

---

## Quick Reference

### Controller URL Map

| Controller | Primary URL | Accessible By | Key Actions |
|------------|-------------|---------------|-------------|
| **AuthController** | `/Auth/Login` | All (before login) | Login, Logout |
| **DashboardController** | `/Dashboard/Index` | All roles | View metrics, recent activity |
| **DocumentController** | `/Document/Index` | All roles | List, view, create documents |
| **WorkflowController** | `/Workflow/Index` | Mayor, Exec Admin | Transition, routing, comments |
| **SignatureController** | `/Signature/Index` | **Mayor ONLY** | Digital signature |
| **OversightController** | `/Oversight/Index` | Oversight, Mayor, Exec Admin | ARTA dashboard, resolve escalations |
| **ExportController** | `/Export/ExportData` | **Oversight ONLY** | CSV export |
| **AdminController** | `/Admin/Index` | Exec Admin, Oversight | User management, audit logs |
| **TrackingController** | `/Tracking/Index` | All authenticated | Document tracking |
| **PublicController** | `/Public/Track` | **No login required** | Public QR tracking |
| **NotificationController** | `/Notification/Index` | All roles | SMS history |

---

### Document Status Quick Guide

| Status | Who Can Set | Next Possible States | Typical Duration |
|--------|-------------|---------------------|------------------|
| Submitted | Records Officer | Endorsed, Returned | < 1 day |
| Endorsed | Executive Admin | UnderReview, Forwarded, ForApproval | < 1 day |
| UnderReview | Executive Admin | ForApproval, Returned | 1-3 days |
| Forwarded | Executive Admin | Accepted, UnderReview | < 1 day |
| Accepted | Department Staff | UnderReview | Same day |
| ForApproval | Executive Admin | Approved, Returned, Rejected | 1-2 days |
| Approved | **Mayor ONLY** | ForRelease | Same day |
| Returned | Mayor, Exec Admin | Submitted (resubmit) | Depends on client |
| Rejected | **Mayor ONLY** | *Terminal - No further states* | - |
| ForRelease | Executive Admin | Completed | 1-2 days |
| Completed | Records Officer | *Terminal - Journey ends* | - |
| Escalated | **System AUTO** | Any (after Oversight resolves) | Varies |

---

### ARTA Period Thresholds

| Period Type | Days | 75% Warning | 90% Critical | 100% Escalation |
|-------------|------|-------------|--------------|-----------------|
| Simple | 3 days | After 2.25 days | After 2.7 days | After 3 days |
| Complex | 7 days | After 5.25 days | After 6.3 days | After 7 days |
| HighlyTechnical | 20 days | After 15 days | After 18 days | After 20 days |

---

### Key System Features by Module

**Module 1: User Authentication & RBAC**
- Cookie-based sessions
- Role-based access control
- Audit trail logging

**Module 2: Document Workflow (FSM)**
- 11-state finite state machine
- Status transition validation
- Workflow history tracking

**Module 3: QR Code Tracking**
- Auto-generation on document creation
- Public tracking without login
- Scan count tracking

**Module 4: SMS Notifications**
- Template-based messages
- Status update alerts
- Delivery status tracking

**Module 5: Digital Signature**
- Canvas-based capture
- SHA-256 integrity verification
- Mayor-exclusive access

**Module 6: ARTA Compliance (Capstone)**
- Automated deadline monitoring
- 3/7/20-day period enforcement
- Smart escalation system
- Compliance reporting

**Module 7: Document Attachments**
- Multiple file uploads
- Secure file storage
- File type validation

**Module 8: Audit Trail**
- Comprehensive action logging
- Security incident tracking
- Compliance reporting

**Module 9: Routing Slips**
- Digital tracking of physical routing
- Sender and receiver tracking
- Action instructions

**Module 10: Data Export**
- CSV export with filters
- Secondary password security
- Export audit logging

---

## Best Practices by Role

### Mayor Best Practices

1. **Daily Review Schedule**
   - Check "Pending Signatures" queue every morning
   - Prioritize documents approaching ARTA deadlines
   - Review ARTA flagged documents immediately

2. **Document Review Process**
   - Read complete document details before signing
   - Check workflow history for any red flags
   - Verify all required department clearances present
   - Use Return (not Reject) when document is salvageable

3. **Signature Management**
   - Keep signature consistent across documents
   - Add meaningful remarks when returning documents
   - Sign documents same day when possible

4. **Communication**
   - Add public comments for client-visible updates
   - Use internal comments for staff instructions
   - Provide specific guidance when returning documents

---

### Executive Admin Best Practices

1. **Document Intake**
   - Verify all required attachments before registering
   - Double-check contact information for SMS accuracy
   - Select appropriate ARTA period (consult service charter)
   - Scan documents at high quality (300 DPI minimum)

2. **Workflow Management**
   - Endorse documents within same day of receipt
   - Use descriptive remarks when forwarding
   - Track documents forwarded to other departments
   - Submit complete documents to Mayor (avoid returns)

3. **Quality Control**
   - Review documents before submitting to Mayor
   - Ensure all department clearances obtained
   - Verify attachments are readable and complete
   - Check for common issues (missing signatures, dates, etc.)

4. **User Administration**
   - Create strong passwords for new users
   - Assign correct role based on job function
   - Deactivate accounts for terminated employees
   - Regular audit of user list for accuracy

5. **System Configuration**
   - Keep department list updated
   - Review workflow step configurations quarterly
   - Update document types as needed
   - Monitor audit logs for unusual activity

---

### Records Officer Best Practices

1. **Client Interaction**
   - Greet clients professionally
   - Explain tracking process clearly
   - Provide written tracking number
   - Demonstrate QR code scanning if needed
   - Set realistic expectations on processing time

2. **Document Registration**
   - Verify completeness before accepting
   - Obtain accurate mobile number for SMS
   - Print and attach QR code immediately
   - Create physical routing slip
   - File physical document securely

3. **Document Tracking**
   - Monitor "Received Today" daily
   - Follow up on returned documents
   - Contact clients proactively for corrections
   - Keep organized filing system

4. **Document Release**
   - Verify client identity before release
   - Require signature in logbook
   - Mark as Completed in system same day
   - Archive file copies properly

5. **Resubmission Handling**
   - Contact client within 24 hours of return
   - Clearly explain required corrections
   - Expedite resubmitted documents
   - Add note documenting corrections made

---

### CART/Oversight Officer Best Practices

1. **Daily ARTA Monitoring**
   - Check dashboard first thing every morning
   - Contact departments with Warning-level documents
   - Escalate Critical documents to department heads
   - Document all intervention attempts

2. **Escalation Management**
   - Investigate root cause thoroughly
   - Document findings in detail
   - Identify systemic issues vs one-off problems
   - Follow up on corrective actions

3. **Report Generation**
   - Generate monthly compliance reports
   - Share with Mayor and department heads
   - Identify trends and patterns
   - Recommend process improvements

4. **Data Export**
   - Export data monthly for backup
   - Use specific filters for targeted analysis
   - Secure exported files appropriately
   - Document export purpose in audit log

5. **Compliance Analysis**
   - Track compliance rate trends over time
   - Identify departments needing support
   - Recommend training or process changes
   - Celebrate improvements and successes

6. **Stakeholder Communication**
   - Present findings to management regularly
   - Provide actionable recommendations
   - Maintain transparency in reporting
   - Balance accountability with support

---

## Training Recommendations

### New User Onboarding (All Roles)

**Week 1: System Fundamentals**
- System overview and ARTA compliance goals
- Login and password security
- Dashboard navigation
- Document viewing and tracking
- SMS notification system

**Week 2: Role-Specific Training**
- Hands-on practice with role functions
- Workflow scenarios and exercises
- Common issues and solutions
- Best practices for role

**Week 3: Supervised Practice**
- Perform tasks under supervision
- Review audit logs of actions
- Q&A and clarification
- Independent practice with spot checks

---

### Ongoing Training

**Monthly**:
- Review of new features/updates
- Common mistakes and how to avoid them
- Success stories and best practices

**Quarterly**:
- ARTA compliance refresher
- Security and audit procedures
- Process improvement workshops
- System performance review

**Annually**:
- Comprehensive system review
- Role-based advanced training
- Security awareness training
- Disaster recovery procedures

---

## System Administration Notes

### For System Administrators

**Database Maintenance**:
- Regular backups (daily recommended)
- Index optimization monthly
- Audit log archival (keep 7 years for compliance)
- Document attachment cleanup (completed + 5 years)

**Performance Monitoring**:
- Monitor background job execution (ARTA escalation runs at midnight)
- Check SMS delivery rates
- Review application logs for errors
- Database query performance

**Security Checks**:
- Review audit logs for suspicious activity
- Monitor failed login attempts
- Check export logs for unauthorized attempts
- Update passwords for service accounts quarterly

**User Account Management**:
- Review active users quarterly
- Deactivate terminated employees immediately
- Enforce password rotation policy
- Monitor role assignments for accuracy

**SMS Configuration**:
- Verify SMS provider credentials
- Monitor SMS quota usage
- Test notification delivery weekly
- Update message templates as needed

**Backup & Recovery**:
- Daily automated database backups
- Weekly full system backups
- Monthly backup restoration tests
- Document recovery procedures

---

## Compliance & Regulatory Considerations

### ARTA (Anti-Red Tape Authority) Compliance

**Republic Act 11032 - Ease of Doing Business Act**

**Key Requirements**:
1. **Processing Periods**: 
   - Simple: 3 working days
   - Complex: 7 working days
   - Highly Technical: 20 working days

2. **Transparency**:
   - Clear tracking of document status
   - Public access to status updates
   - Documented reason for delays

3. **Accountability**:
   - Identification of responsible office
   - Escalation for non-compliance
   - Resolution documentation

**TrackNGo Compliance Features**:
- ✅ Automated deadline monitoring
- ✅ Public QR code tracking
- ✅ Escalation with responsible office identification
- ✅ Complete audit trail
- ✅ SMS notifications for transparency
- ✅ Compliance rate reporting

---

### Data Privacy Act (RA 10173)

**Personal Information Protection**:

**Collected Data**:
- User full names, usernames, passwords (hashed)
- Mobile numbers and email addresses
- Document submitter information
- IP addresses in audit logs

**TrackNGo Safeguards**:
- Password hashing (SHA-256)
- Role-based access control
- Audit trail of all data access
- Secondary password for data export
- No sharing of personal data with third parties
- Data retention policy (7 years for government compliance)

**User Rights**:
- Right to access own data
- Right to correction of inaccurate data
- Right to erasure (after retention period)
- Right to data portability (via export function)

---

### Government Audit Requirements

**Commission on Audit (COA) Compliance**:

**Required Records**:
- Complete audit trail (all system actions)
- Document processing timelines
- User activity logs
- Export audit logs
- Digital signature records

**TrackNGo Features**:
- ✅ Permanent audit log storage
- ✅ Export function for audit reports
- ✅ Tamper-evident workflow history
- ✅ Digital signature with SHA-256 verification
- ✅ Timestamped records (all actions)

**Retention Period**: 7 years (as per government regulations)

---

### Electronic Document Security

**Digital Signature Integrity**:
- Canvas-based capture
- SHA-256 hash generation
- Hash stored with signature
- Verification against document changes
- Signature image storage

**Document Attachment Security**:
- Server-side file storage
- Access control via authentication
- File type validation
- Virus scanning recommended (external integration)
- Secure deletion after retention period

---

## Integration Points

### Potential System Integrations

**1. SMS Gateway Integration**
**Current**: Configured in appsettings.json
**Providers**: Semaphore, Twilio, Nexmo, local telco APIs
**Configuration**:
```json
{
  "SMSSettings": {
    "Provider": "Semaphore",
    "APIKey": "your-api-key",
    "SenderName": "TrackNGo"
  }
}
```

**2. Email Notification Integration**
**Purpose**: Backup to SMS, notifications for users without mobile
**Implementation**: SMTP configuration
**Use Cases**: 
- Daily digest for Mayor (pending signatures)
- Weekly compliance report for Oversight
- Account activation emails

**3. Document Management System Integration**
**Purpose**: Link to existing DMS for archival
**Method**: API endpoints for document metadata
**Benefits**: Unified document repository

**4. E-Signature Integration**
**Purpose**: Legal-grade digital signatures
**Providers**: DocuSign, Adobe Sign, local e-signature providers
**Requirements**: Digital certificate infrastructure

**5. Government Portal Integration**
**Purpose**: Single sign-on with gov't portal
**Method**: OAuth 2.0 / SAML integration
**Benefits**: Unified authentication, reduced password fatigue

**6. Business Intelligence Integration**
**Purpose**: Advanced analytics and visualization
**Tools**: Power BI, Tableau
**Data Source**: Export function or direct database connection
**Use Cases**: 
- Executive dashboards
- Predictive analytics for bottlenecks
- Trend analysis

**7. Mobile Application**
**Purpose**: Staff access on-the-go
**Features**:
- Document approval via mobile
- QR code scanning
- Push notifications
- Offline mode for tracking

---

## Frequently Asked Questions

### General Questions

**Q: Can clients login to the system?**
A: No. Clients track documents publicly via QR code without login. This is intentional for ease of access.

**Q: How long are documents stored in the system?**
A: Active documents remain indefinitely. Audit logs and completed documents retained for 7 years per government regulations.

**Q: Can deleted documents be recovered?**
A: TrackNGo does not have document deletion function. Documents remain in Completed/Rejected status permanently for audit purposes.

**Q: Is internet required for the system?**
A: Yes, TrackNGo is a web-based system. However, QR codes work offline if the URL is cached.

---

### Role-Specific Questions

**Q: Can Executive Admin sign documents?**
A: No. Digital signature is exclusive to Mayor role. This enforces proper authority hierarchy.

**Q: Can Records Officer see all documents?**
A: Yes, read-only access. Records Officers can view but cannot modify workflow status (except Complete and Resubmit).

**Q: Can multiple users have Mayor role?**
A: Technically yes, but recommended: one Mayor account + Vice Mayor as backup. All signature actions are individually logged.

**Q: What if Oversight Officer is unavailable to resolve escalation?**
A: System Administrator can temporarily grant Oversight role to another user. Document remains locked until resolution.

---

### Technical Questions

**Q: What happens if SMS fails to send?**
A: SMS status marked as "Failed" in database. System retries 3 times. Manual resend available in Notification History.

**Q: Can tracking numbers be customized?**
A: Yes, format is configurable in code. Default: TNG-YYYY-NNNN. Can change to match LGU naming convention.

**Q: How are working days calculated?**
A: System excludes Saturdays, Sundays, and holidays defined in configuration. Holiday calendar should be updated annually.

**Q: Is the system mobile-responsive?**
A: Yes, all views are responsive. Public tracking page optimized for mobile devices.

**Q: Can the system handle multiple languages?**
A: Current version is English/Filipino. Localization can be added via resource files.

---

### Workflow Questions

**Q: Can a document skip workflow steps?**
A: No, FSM enforces sequential processing. Exception: Returned documents can be fast-tracked by same staff.

**Q: What if document is sent to wrong department?**
A: Executive Admin can forward to correct department. Workflow history shows all routing changes.

**Q: Can Mayor delegate signature authority?**
A: Not in current system. Proper delegation requires creating second Mayor-role account (e.g., Vice Mayor).

**Q: What happens to documents during Mayor absence?**
A: Documents queue in ForApproval status. System can be configured to send alerts after X days pending.

---

## Appendix A: Database Schema Overview

### Core Tables

**1. Users** (Table 2)
- Id (PK)
- Username, PasswordHash
- FullName, Email, PhoneNumber, MobileNumber
- Role (Mayor, ExecutiveAdmin, RecordsOfficer, OversightOfficer)
- DepartmentId (FK)
- IsActive, CreatedAt, LastLoginAt

**2. Documents** (Table 1 - Core Entity)
- Id (PK)
- TrackingNumber (TNG-YYYY-NNNN)
- Title, DocumentType, TypeId (FK)
- OriginatingDepartment, DepartmentId (FK)
- SubmittedBy, SubmittedByUserId (FK)
- ContactNumber, EmailAddress
- CurrentStatus (FSM state), CurrentOfficeName
- CurrentStepIndex, TotalSteps
- AttachmentPath, QRCodePath
- DateFiled, DateCompleted, LastUpdated
- ARTAProcessingDays, EscalationDeadline
- IsEscalated, IsLocked
- CreatedByUserId (FK)

**3. WorkflowTransitions** (Table 9 - FSM History)
- Id (PK)
- DocumentId (FK)
- FromStatus, ToStatus
- FromOffice, ToOffice
- ActionTaken, Remarks
- PerformedByUserId (FK)
- TransitionDate

**4. DocumentComments** (Table 11)
- Id (PK)
- DocumentId (FK)
- UserId (FK)
- Content
- IsInternal (true = staff only, false = public)
- CreatedAt

**5. DigitalSignatures** (Table 7)
- Id (PK)
- DocumentId (FK)
- SignedByUserId (FK)
- SignatureImagePath
- SignatureHash (SHA-256)
- ActionType (Approved/Rejected)
- Remarks
- SignedAt

**6. AuditTrailEntry** (Table 3)
- Id (PK)
- DocumentId (FK, nullable)
- UserId (FK)
- Action (UserLogin, DocumentCreated, StatusChanged, etc.)
- Description
- IPAddress
- Timestamp

**7. SMSNotification** (Table 10)
- Id (PK)
- DocumentId (FK)
- RecipientNumber
- MessageContent
- Status (Queued, Sent, Delivered, Failed)
- SentAt, DeliveredAt
- ErrorMessage

**8. QRCodeRecord** (Table 15)
- Id (PK)
- DocumentId (FK)
- QRCodePath
- GeneratedAt
- ScanCount

**9. RoutingSlip** (Table 16)
- Id (PK)
- DocumentId (FK)
- TrackingNumber
- ReceivedByUserId (FK)
- DateReceived
- SenderName
- ActionInstruction
- TargetDepartmentId (FK)
- NotedByUserId (FK)
- SlipStatus (Active, Completed, Returned)

**10. EscalationLog** (Table 12)
- Id (PK)
- DocumentId (FK)
- EscalatedDate
- ResolvedDate
- ResolvedByUserId (FK)
- EscalationLevel (Warning, Critical, Overdue)
- ResponsibleOffice
- ResolutionNotes

**11. ExportAuditLog** (Table 14)
- Id (PK)
- ExportedByUserId (FK)
- ExportType (CSV, PDF)
- Scope (filters applied)
- DataSize (bytes)
- Status (Success, Failed)
- ExportDate
- IPAddress

**12. Departments** (Table 5)
- Id (PK)
- DepartmentName
- DepartmentCode
- DepartmentHead
- IsActive

**13. DocumentTypeConfig** (Table 4)
- Id (PK)
- TypeName (Letter, Memorandum, Resolution, etc.)
- Description
- DefaultARTAPeriod
- RequiresSignature
- IsActive

**14. WorkflowStep** (Table 8)
- Id (PK)
- TypeId (FK to DocumentTypeConfig)
- StepNumber
- StepName
- AssignedDepartmentId (FK)
- AllowedActions

**15. DocumentAttachment** (Table 6 - Multi-file support)
- Id (PK)
- DocumentId (FK)
- FileName
- FilePath
- FileSize
- FileType
- UploadedAt

**16. DocumentMetadata** (Table 13)
- Id (PK)
- DocumentId (FK)
- OriginDepartmentId (FK)
- TargetDepartmentId (FK)
- PriorityLevel
- Confidentiality
- Remarks

**17. ReportLog** (Table 17)
- Id (PK)
- GeneratedByUserId (FK)
- ReportType (DepartmentPerformance, Workflow, DocumentSummary)
- Parameters
- GeneratedAt

---

## Appendix B: System Configuration Files

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TrackNGoDB;Trusted_Connection=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "SMSSettings": {
    "Provider": "Semaphore",
    "APIKey": "your-api-key-here",
    "SenderName": "TrackNGo",
    "APIUrl": "https://api.semaphore.co/api/v4/messages"
  },
  "ARTASettings": {
    "SimpleDays": 3,
    "ComplexDays": 7,
    "HighlyTechnicalDays": 20,
    "WarningThreshold": 75,
    "CriticalThreshold": 90,
    "EscalationJobCron": "0 0 * * *"
  },
  "QRCodeSettings": {
    "BaseUrl": "https://yourLGU.gov.ph",
    "ImageSize": 300,
    "StoragePath": "/wwwroot/qrcodes/"
  },
  "SecuritySettings": {
    "SessionTimeout": 30,
    "PasswordMinLength": 6,
    "RequirePasswordChange": false,
    "PasswordExpirationDays": 90
  },
  "FileUploadSettings": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": ".pdf,.docx,.xlsx,.jpg,.png",
    "UploadPath": "/wwwroot/uploads/"
  },
  "ExportSettings": {
    "ExportPasswordHash": "hashed-export-password-here",
    "MaxRecordsPerExport": 10000
  }
}
```

---

## Appendix C: Workflow State Transition Matrix

| From Status | To Status | Who Can Perform | Remarks Required | Business Rule |
|-------------|-----------|-----------------|------------------|---------------|
| Submitted | Endorsed | Executive Admin | No | Document classified |
| Submitted | Returned | Executive Admin, Mayor | Yes | Needs correction |
| Endorsed | UnderReview | Executive Admin | No | Assigned for review |
| Endorsed | Forwarded | Executive Admin | No | Sent to department |
| Endorsed | ForApproval | Executive Admin | No | Ready for Mayor |
| UnderReview | Endorsed | Executive Admin | No | Review complete |
| UnderReview | Returned | Executive Admin | Yes | Needs revision |
| Forwarded | Accepted | Department Staff | No | Department accepts |
| Accepted | UnderReview | Executive Admin | No | Begin review |
| ForApproval | Approved | **Mayor ONLY** | No | Digital signature |
| ForApproval | Rejected | **Mayor ONLY** | Recommended | Permanent rejection |
| ForApproval | Returned | **Mayor ONLY** | **MANDATORY** | Needs correction |
| Approved | ForRelease | Executive Admin | No | Prepare for release |
| ForRelease | Completed | Records Officer | No | Released to client |
| Returned | Submitted | Records Officer | No | Resubmitted |
| Escalated | Any | Executive Admin (after Oversight resolves) | No | Must be unlocked first |

**Special Rules**:
- Status: Rejected → Terminal state (no further transitions)
- Status: Completed → Terminal state (no further transitions)
- Status: Escalated → IsLocked = true (blocks all transitions until resolved)
- Action: Return → Remarks are MANDATORY (system validates)

---

## Appendix D: SMS Notification Templates

### Template 1: Document Received
**Trigger**: Document created in system
**Recipient**: Document submitter
**Message**:
```
TrackNGo: Your document {TrackingNumber} has been received by {LGU_Name}. 
Track status at: {TrackingURL}
```

### Template 2: Status Update
**Trigger**: Document status changes
**Recipient**: Document submitter
**Message**:
```
TrackNGo: Update on {TrackingNumber}. Status: {NewStatus}. 
Current office: {CurrentOfficeName}. Track: {TrackingURL}
```

### Template 3: Document Returned
**Trigger**: Mayor or Admin returns document
**Recipient**: Document submitter
**Message**:
```
TrackNGo: Your document {TrackingNumber} has been returned. 
Reason: {Remarks}. Please resubmit corrected version to Records Office.
```

### Template 4: Document Approved
**Trigger**: Mayor approves document
**Recipient**: Document submitter
**Message**:
```
TrackNGo: Good news! Your document {TrackingNumber} has been approved 
and will be ready for release soon. Track: {TrackingURL}
```

### Template 5: Ready for Pickup
**Trigger**: Document status changes to ForRelease
**Recipient**: Document submitter
**Message**:
```
TrackNGo: Your document {TrackingNumber} is ready for pickup at 
{LGU_Name} Records Office. Bring valid ID. Office hours: {OfficeHours}
```

### Template 6: Document Completed
**Trigger**: Document marked as Completed
**Recipient**: Document submitter
**Message**:
```
TrackNGo: Your document {TrackingNumber} has been released. 
Thank you for using our tracking system. Processing time: {DaysElapsed} days.
```

### Template 7: ARTA Escalation
**Trigger**: Document exceeds ARTA deadline
**Recipients**: Document submitter + all stakeholders
**Message**:
```
TrackNGo ALERT: Document {TrackingNumber} has exceeded the {ARTAPeriod}-day 
processing period. Current office: {ResponsibleOffice}. 
Oversight team has been notified.
```

### Template 8: Forwarded to Department
**Trigger**: Document forwarded to department
**Recipient**: Department head
**Message**:
```
TrackNGo: Document {TrackingNumber} ({Title}) has been forwarded to 
{DepartmentName}. Please review. Track: {TrackingURL}
```

---

## Appendix E: Report Templates

### Department Performance Report

**Header Information**:
- Report Title: Department Performance Report
- Date Range: [From Date] to [To Date]
- Generated By: [User Name]
- Generated Date: [Timestamp]
- LGU Name: [Municipality/City Name]

**Summary Metrics**:
- Total Departments: [Count]
- Average Compliance Rate: [Percentage]
- Best Performing Department: [Name]
- Needs Improvement: [Name]

**Department Details Table**:

| Department | Total Docs | On Time | Escalated | Avg Days | Compliance % | Rank |
|------------|------------|---------|-----------|----------|--------------|------|
| Engineering | 45 | 42 | 3 | 5.2 | 93.3% | 1 |
| Legal | 32 | 28 | 4 | 6.8 | 87.5% | 2 |
| Finance | 28 | 24 | 4 | 7.1 | 85.7% | 3 |

**Recommendations**:
- Departments below 90% compliance require intervention
- Identify common bottlenecks
- Propose training or resource allocation

---

### Workflow Analysis Report

**Header Information**:
- Report Title: Workflow Analysis Report
- Document Type: [All/Specific Type]
- Date Range: [From Date] to [To Date]
- Generated By: [User Name]

**Average Processing Time by Stage**:

| Workflow Stage | Avg Days | Min Days | Max Days | Std Dev | Sample Size |
|----------------|----------|----------|----------|---------|-------------|
| Submitted → Endorsed | 0.5 | 0.1 | 2.0 | 0.3 | 150 |
| Endorsed → UnderReview | 1.2 | 0.2 | 5.0 | 0.8 | 150 |
| UnderReview → ForApproval | 2.8 | 1.0 | 8.0 | 1.5 | 150 |
| ForApproval → Approved | 1.5 | 0.5 | 7.0 | 1.2 | 145 |
| Approved → Completed | 1.0 | 0.5 | 3.0 | 0.5 | 145 |
| **Total Average** | **7.0** | - | - | - | **145** |

**Bottleneck Analysis**:
- Longest Stage: UnderReview → ForApproval (2.8 days avg)
- Most Variable: UnderReview → ForApproval (std dev 1.5)
- Recommendation: Streamline departmental review process

---

### Document Summary Report

**Header Information**:
- Report Title: Monthly Document Summary
- Period: [Month Year]
- Generated By: [User Name]
- Generated Date: [Timestamp]

**Overview Statistics**:
- Total Documents Received: 150
- Total Completed: 145
- Total In Progress: 5
- Total Returned: 8
- Total Rejected: 2
- Total Escalated: 3

**Status Distribution**:
- Submitted: 2 (1.3%)
- Endorsed: 1 (0.7%)
- Under Review: 2 (1.3%)
- For Approval: 0 (0%)
- Approved: 0 (0%)
- For Release: 0 (0%)
- Completed: 145 (96.7%)
- Returned: 0 (0%)
- Rejected: 2 (1.3%)
- Escalated: 0 (0%)

**ARTA Compliance**:
- Simple (3 days): 50 received, 48 on time (96%)
- Complex (7 days): 80 received, 75 on time (93.75%)
- Highly Technical (20 days): 20 received, 19 on time (95%)
- **Overall Compliance Rate: 94.7%**

**Document Type Breakdown**:
- Business Permits: 40
- Building Permits: 35
- Certifications: 30
- Letters: 25
- Resolutions: 20

**Top Performing Departments**:
1. Records Office: 100% compliance
2. Engineering: 93% compliance
3. Legal: 88% compliance

---

## Appendix F: Security Incident Response

### Incident Types & Response

**1. Unauthorized Access Attempt**

**Detection**:
- Multiple failed login attempts from same IP
- Login from unusual location/time
- Access to restricted modules

**Response**:
1. Check audit logs for details
2. Verify legitimate user or attack
3. Lock account if compromised
4. Reset password
5. Document incident
6. Report to IT security team

---

**2. Data Export Violation**

**Detection**:
- Failed export attempts with wrong password
- Export of large datasets outside normal pattern
- Export during unusual hours

**Response**:
1. Review ExportAuditLog
2. Verify authorization
3. Check data scope exported
4. Interview user if suspicious
5. Revoke export permission if necessary
6. Document incident

---

**3. Workflow Manipulation**

**Detection**:
- Document status changed without proper transition
- Workflow bypassed
- Unauthorized role escalation

**Response**:
1. Review AuditTrailEntry for affected documents
2. Identify user who performed action
3. Verify if legitimate administrative override
4. Restore correct workflow state if needed
5. Revoke permissions if abuse detected
6. Document and report

---

**4. Signature Forgery**

**Detection**:
- Signature hash mismatch
- Signature applied by non-Mayor user
- Multiple signatures on same document

**Response**:
1. Immediately lock document
2. Verify signature authenticity
3. Check DigitalSignatures table
4. Interview Mayor to confirm signature
5. Void document if forgery confirmed
6. Legal action if criminal activity

---

**5. Database Breach**

**Detection**:
- Unusual database queries
- Direct database access from unknown IP
- Data modification outside application

**Response**:
1. **IMMEDIATE**: Disconnect database from network
2. Lock all user accounts
3. Engage IT security/forensics team
4. Review database logs
5. Identify compromised data
6. Notify affected parties
7. Restore from clean backup
8. Document and report to authorities

---

## Appendix G: Disaster Recovery Procedures

### Backup Strategy

**Daily Backups**:
- Database full backup at 2:00 AM
- Transaction log backup every 4 hours
- Retention: 30 days

**Weekly Backups**:
- Full system backup (database + files)
- Sunday at 12:00 AM
- Retention: 90 days

**Monthly Backups**:
- Archive backup
- First day of month
- Retention: 7 years (compliance)

**Backup Storage**:
- Primary: On-premises server
- Secondary: Off-site cloud storage
- Tertiary: External hard drive (air-gapped)

---

### Recovery Scenarios

**Scenario 1: Database Corruption**

**Symptoms**:
- Application errors
- Data inconsistency
- SQL errors in logs

**Recovery Steps**:
1. Stop application service
2. Assess corruption extent
3. Attempt database repair tools
4. If repair fails: Restore from latest backup
5. Restore transaction logs to minimize data loss
6. Verify data integrity
7. Restart application
8. Notify users of downtime
9. Document incident

**Expected Downtime**: 1-4 hours

---

**Scenario 2: Server Hardware Failure**

**Symptoms**:
- Server unresponsive
- Hardware error alerts
- Complete system failure

**Recovery Steps**:
1. Assess hardware damage
2. Prepare replacement server
3. Install OS and application
4. Restore database from backup
5. Restore file attachments
6. Update DNS/network config
7. Test all system functions
8. Resume operations
9. Document incident

**Expected Downtime**: 4-24 hours

---

**Scenario 3: Ransomware Attack**

**Symptoms**:
- Files encrypted
- Ransom note displayed
- System slowdown

**Recovery Steps**:
1. **IMMEDIATE**: Disconnect from network
2. DO NOT pay ransom
3. Isolate infected systems
4. Engage cybersecurity experts
5. Restore from clean, pre-infection backup
6. Scan all systems for malware
7. Patch vulnerabilities
8. Resume operations on clean infrastructure
9. Report to law enforcement

**Expected Downtime**: 1-3 days

---

**Scenario 4: Natural Disaster (Fire/Flood)**

**Symptoms**:
- Physical destruction of data center
- Complete loss of on-premises infrastructure

**Recovery Steps**:
1. Activate disaster recovery plan
2. Set up temporary infrastructure
3. Restore from off-site cloud backup
4. Establish temporary operations
5. Communicate with stakeholders
6. Plan long-term rebuild
7. Document incident

**Expected Downtime**: 3-14 days

---

### Recovery Testing

**Quarterly Tests**:
- Restore database backup to test environment
- Verify data integrity
- Test application functionality
- Document restoration time

**Annual Tests**:
- Full disaster recovery simulation
- Restore to alternate infrastructure
- Test all recovery procedures
- Update recovery plan based on findings

---

## Document Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-06-28 | System Team | Initial comprehensive guide creation |
| | | | All role processes documented |
| | | | Module access matrix completed |
| | | | Workflow scenarios added |
| | | | Appendices with technical details |

---

## Acknowledgments

This guide was created to support the successful implementation and operation of the TrackNGo Document Tracking System in compliance with:
- Republic Act 11032 (Ease of Doing Business and Efficient Government Service Delivery Act of 2018)
- Republic Act 10173 (Data Privacy Act of 2012)
- Commission on Audit (COA) regulations
- Local Government Unit best practices

---

**For questions or support, contact**:
- System Administrator: [admin@yourLGU.gov.ph]
- IT Support: [support@yourLGU.gov.ph]
- CART/Oversight Officer: [cart@yourLGU.gov.ph]

**System Version**: TrackNGo v1.0  
**Last Updated**: June 28, 2026

---

**END OF DOCUMENT**
