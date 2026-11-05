/* ============================================================
SEED SCRIPT (SSMS "Script as INSERT" style)
Complements Auditor/Models/Seed DB.sql for the current schema.
Ordered to satisfy FK dependencies. Safe to run once against an
empty Auditor database (airport already has row id=1, so it is
NOT re-inserted here).
============================================================ */

/* ============================================================
DEPARTMENT
============================================================ */
SET IDENTITY_INSERT [department] ON
GO
INSERT [department] ([id], [name], [description]) VALUES (1, N'Operations', N'Airport Operations')
GO
SET IDENTITY_INSERT [department] OFF
GO

/* ============================================================
APP_USER
password_hash = SHA256/Base64 of "Test@123" (matches AuthController.HashPassword)
============================================================ */
SET IDENTITY_INSERT [app_user] ON
GO
INSERT [app_user] ([id], [azure_ad_object_id], [display_name], [email], [is_active], [created_date], [password_hash], [role])
VALUES (1, NEWID(), N'Test Auditor', N'auditor@test.com', 1, SYSDATETIME(), N'h3bxCOJHqx4rMjBCwEnCZkB8gfutQb3h6N/Bu2b9Jn4=', N'Auditor')
GO
SET IDENTITY_INSERT [app_user] OFF
GO

/* ============================================================
QUESTION_CATEGORY
============================================================ */
SET IDENTITY_INSERT [question_category] ON
GO
INSERT [question_category] ([id], [name], [description]) VALUES (1, N'Cleanliness', N'Cleanliness inspection')
GO
INSERT [question_category] ([id], [name], [description]) VALUES (2, N'Safety', N'Safety checks')
GO
INSERT [question_category] ([id], [name], [description]) VALUES (3, N'Facilities', N'Facility condition')
GO
SET IDENTITY_INSERT [question_category] OFF
GO

/* ============================================================
QUESTION_TYPE
============================================================ */
SET IDENTITY_INSERT [question_type] ON
GO
INSERT [question_type] ([id], [name], [description]) VALUES (1, N'YES_NO', N'Compliance yes or no')
GO
INSERT [question_type] ([id], [name], [description]) VALUES (2, N'MULTIPLE_CHOICE', N'Multiple choice')
GO
INSERT [question_type] ([id], [name], [description]) VALUES (3, N'TEXT', N'Text answer')
GO
INSERT [question_type] ([id], [name], [description]) VALUES (4, N'NUMBER', N'Numeric answer')
GO
INSERT [question_type] ([id], [name], [description]) VALUES (5, N'RATING', N'Rating scale')
GO
INSERT [question_type] ([id], [name], [description]) VALUES (6, N'BOOLEAN', N'True false')
GO
INSERT [question_type] ([id], [name], [description]) VALUES (7, N'CHECKLIST', N'Checklist options')
GO
INSERT [question_type] ([id], [name], [description]) VALUES (8, N'DATE', N'Date value')
GO
INSERT [question_type] ([id], [name], [description]) VALUES (9, N'TIME', N'Time value')
GO
INSERT [question_type] ([id], [name], [description]) VALUES (10, N'PHOTO', N'Photo evidence')
GO
SET IDENTITY_INSERT [question_type] OFF
GO

/* ============================================================
QUESTION_BANK
============================================================ */
SET IDENTITY_INSERT [question_bank] ON
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (1, N'Are washroom floors clean?', 1, 1, N'Clean every 30 minutes', N'Cleaning Contractor', 1)
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (2, N'Are emergency exits accessible?', 2, 1, N'Exit must always be clear', N'Security Contractor', 1)
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (3, N'Is lighting functioning correctly?', 3, 1, N'Lights must be operational', N'Maintenance Contractor', 1)
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (4, N'Rate overall washroom cleanliness', 1, 5, N'Minimum rating 4', N'Cleaning Contractor', 1)
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (5, N'How many waste bins are available?', 3, 4, N'Minimum 3 bins required', N'Maintenance Contractor', 1)
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (6, N'Describe any visible damages', 3, 3, N'Report damages', N'Maintenance Contractor', 1)
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (7, N'Select observed safety issues', 2, 7, N'Report hazards', N'Security Contractor', 1)
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (8, N'Date of last maintenance inspection', 3, 8, N'Monthly inspection', N'Maintenance Contractor', 1)
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (9, N'Time cleaning was performed', 1, 9, N'Every 30 minutes', N'Cleaning Contractor', 1)
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [created_by])
VALUES (10, N'Upload photo of inspected area', 1, 10, N'Photo evidence required', N'Cleaning Contractor', 1)
GO
SET IDENTITY_INSERT [question_bank] OFF
GO

/* ============================================================
QUESTION_BANK_OPTION
requires_reason = 1 on the NO options, matching the reasons below
============================================================ */
SET IDENTITY_INSERT [question_bank_option] ON
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (1, 1, N'YES', N'YES', 1, 0)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (2, 1, N'NO', N'NO', 2, 1)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (3, 2, N'YES', N'YES', 1, 0)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (4, 2, N'NO', N'NO', 2, 1)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (5, 3, N'YES', N'YES', 1, 0)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (6, 3, N'NO', N'NO', 2, 1)
GO
SET IDENTITY_INSERT [question_bank_option] OFF
GO

/* ============================================================
QUESTION_BANK_REASON
============================================================ */
SET IDENTITY_INSERT [question_bank_reason] ON
GO
INSERT [question_bank_reason] ([id], [question_bank_id], [reason_text]) VALUES (1, 1, N'Dirty floor')
GO
INSERT [question_bank_reason] ([id], [question_bank_id], [reason_text]) VALUES (2, 1, N'Bad smell')
GO
INSERT [question_bank_reason] ([id], [question_bank_id], [reason_text]) VALUES (3, 1, N'Overflowing bins')
GO
INSERT [question_bank_reason] ([id], [question_bank_id], [reason_text]) VALUES (4, 2, N'Exit blocked')
GO
INSERT [question_bank_reason] ([id], [question_bank_id], [reason_text]) VALUES (5, 2, N'Objects blocking path')
GO
INSERT [question_bank_reason] ([id], [question_bank_id], [reason_text]) VALUES (6, 3, N'Light bulb broken')
GO
INSERT [question_bank_reason] ([id], [question_bank_id], [reason_text]) VALUES (7, 3, N'Electrical fault')
GO
SET IDENTITY_INSERT [question_bank_reason] OFF
GO

/* ============================================================
AUDIT_TEMPLATE
============================================================ */
SET IDENTITY_INSERT [audit_template] ON
GO
INSERT [audit_template] ([id], [name], [description], [version], [is_active], [created_by])
VALUES (1, N'Daily Airport Inspection', N'Standard daily inspection', N'1.0', 1, 1)
GO
SET IDENTITY_INSERT [audit_template] OFF
GO

/* ============================================================
AUDIT_TEMPLATE_ITEM
============================================================ */
SET IDENTITY_INSERT [audit_template_item] ON
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (1, 1, 1, 1, 1)
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (2, 1, 2, 2, 1)
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (3, 1, 3, 3, 1)
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (4, 1, 4, 4, 0)
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (5, 1, 5, 5, 0)
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (6, 1, 6, 6, 0)
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (7, 1, 7, 7, 0)
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (8, 1, 8, 8, 0)
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (9, 1, 9, 9, 0)
GO
INSERT [audit_template_item] ([id], [template_id], [question_bank_id], [sequence], [mandatory]) VALUES (10, 1, 10, 10, 0)
GO
SET IDENTITY_INSERT [audit_template_item] OFF
GO

/* ============================================================
AUDIT_SITE
airport_id = 1 references the existing airport row (O. R. Tambo / ORTIA)
============================================================ */
SET IDENTITY_INSERT [audit_site] ON
GO
INSERT [audit_site] ([id], [name], [location], [status], [airport_id], [department_id], [created_by])
VALUES (1, N'Terminal A Washrooms', N'Terminal A', N'ACTIVE', 1, 1, 1)
GO
SET IDENTITY_INSERT [audit_site] OFF
GO

/* ============================================================
AUDIT_SITE_TEMPLATE (join table, no identity)
============================================================ */
INSERT [audit_site_template] ([site_id], [template_id]) VALUES (1, 1)
GO

/* ============================================================
AUDIT_SCHEDULE
============================================================ */
SET IDENTITY_INSERT [audit_schedule] ON
GO
INSERT [audit_schedule] ([id], [template_id], [site_id], [scheduler_id], [scheduled_date], [due_date], [status])
VALUES (1, 1, 1, 1, GETDATE(), DATEADD(DAY, 1, GETDATE()), N'SCHEDULED')
GO
SET IDENTITY_INSERT [audit_schedule] OFF
GO

/* ============================================================
AUDIT_SCHEDULE_AUDITOR (join table, no identity)
============================================================ */
INSERT [audit_schedule_auditor] ([schedule_id], [auditor_id]) VALUES (1, 1)
GO
