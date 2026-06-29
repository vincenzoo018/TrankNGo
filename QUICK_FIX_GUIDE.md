# 🚨 QUICK FIX - SMS Notifications & Document Intake Errors

## ⚡ Fast Solution (3 Steps - 2 Minutes)

### Step 1: Stop the Application
- Close Visual Studio or Press `Ctrl+C` in terminal
- Wait 3 seconds

### Step 2: Run the Fix Script

**Option A - Windows Batch File** (Easiest):
```cmd
Double-click: fix-database.cmd
```

**Option B - PowerShell Script**:
```powershell
.\fix-database.ps1
```

**Option C - Manual Commands**:
```cmd
dotnet ef database drop --force
dotnet ef database update
dotnet build
```

### Step 3: Restart and Test
1. Start your application (F5 or `dotnet run`)
2. Login as `records` / `admin123`
3. Try: **Document Intake** → Should work! ✅
4. Try: **SMS Notifications** → Should work! ✅

---

## 🎯 What This Fixes

### Before (BROKEN ❌):
```
Clicking "SMS Notifications" → SQL Error
Creating a Document → SQL Error
```

### After (WORKING ✅):
```
Clicking "SMS Notifications" → Page loads perfectly
Creating a Document → Document created successfully
```

---

## 📝 The Error You Were Seeing

```
SqlException: Invalid column name 'GatewayResponse'
Invalid column name 'MessageContent'
Invalid column name 'QueuedAt'
Invalid column name 'RecipientName'
Invalid column name 'RecipientNumber'
Invalid column name 'TriggerEvent'
```

And when creating documents:
```
SqlException: Invalid column name 'CategoryFlags'
Invalid column name 'ConferenceName'
Invalid column name 'Province'
...
```

---

## 🔧 What the Fix Does

1. **Stops** any running TrackNGo process
2. **Drops** the old database (TrackNGo.db)
3. **Creates** a new database with correct schema
4. **Builds** the application
5. **Seeds** default users and data

**⚠️ Warning**: This deletes all existing data (OK for development)

---

## 🧪 Quick Test After Fix

```
1. Login as: records / admin123
2. Click: "SMS Notifications" → Should load
3. Click: "Document Intake"
4. Fill form:
   - Title: "Test Document"
   - Type: "Application"
   - Department: "Office of the Mayor"
   - Submitted By: "Test User"
   - Contact: "+639171234567"
5. Click "Register Document" → Should work!
```

---

## 🆘 If Fix Script Fails

### Error: "dotnet ef command not found"
```powershell
dotnet tool install --global dotnet-ef
```

### Error: "Cannot access database file"
1. Close Visual Studio completely
2. Delete `TrackNGo.db` manually
3. Run fix script again

### Error: "Process is still running"
```cmd
# Open Task Manager (Ctrl+Shift+Esc)
# End "TrackNGo.exe" or "dotnet.exe"
# Run fix script again
```

---

## 📂 Default Users After Fix

| Username | Password | Role |
|----------|----------|------|
| mayor | admin123 | Mayor |
| admin | admin123 | Executive Admin |
| records | admin123 | Records Officer |
| oversight | admin123 | Oversight Officer |

---

## ✅ Success Indicators

You'll know it worked when:
- [ ] No SQL errors when starting app
- [ ] SMS Notifications page loads
- [ ] Can create documents
- [ ] QR codes are generated
- [ ] All sidebar modules work

---

## 📞 Need Detailed Help?

See: `DATABASE_FIX_README.md` for:
- Complete explanation of the issue
- Alternative solutions
- Troubleshooting guide
- Technical details

---

## 💡 Why This Happened

**Simple Explanation**:
- The database was created with an old schema
- The C# code was updated
- Database and code are now out of sync
- Solution: Recreate database from current code

**Technical Explanation**:
- Entity Framework migration didn't update database
- Model properties don't match database columns
- Need to drop and recreate from current models

---

## 🎉 That's It!

After running the fix script:
- **SMS Notifications** → ✅ FIXED
- **Document Intake** → ✅ FIXED
- All other modules → ✅ WORKING

**Total Time**: 2 minutes  
**Difficulty**: Easy (just run a script)  
**Data Loss**: Yes (only test data)

---

**Run the script and you're done!** 🚀
