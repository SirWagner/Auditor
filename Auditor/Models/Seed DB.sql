/* ============================================================
E2E DATABASE RESET
============================================================ */

DELETE FROM audit_response_reason;
DELETE FROM audit_response;
DELETE FROM audit_attachment;
DELETE FROM audit_execution;
DELETE FROM audit_schedule_auditor;
DELETE FROM audit_schedule;
DELETE FROM audit_site_template;
DELETE FROM audit_site;
DELETE FROM audit_template_item;
DELETE FROM audit_template;

DELETE FROM question_bank_reason;
DELETE FROM question_bank_option;
DELETE FROM question_bank;

DELETE FROM question_category;
DELETE FROM question_type;

DELETE FROM app_user;
DELETE FROM department;
DELETE FROM airport;



/* ============================================================
AIRPORT
============================================================ */

SET IDENTITY_INSERT airport ON

INSERT INTO airport (id,name,code,location) VALUES
(1,'OR Tambo International','ORT','Johannesburg')

SET IDENTITY_INSERT airport OFF



/* ============================================================
DEPARTMENT
============================================================ */

SET IDENTITY_INSERT department ON

INSERT INTO department (id,name,description,airport_id) VALUES
(1,'Operations','Airport Operations',1)

SET IDENTITY_INSERT department OFF



/* ============================================================
USER
============================================================ */

SET IDENTITY_INSERT app_user ON

INSERT INTO app_user
(id,azure_ad_object_id,display_name,email,airport_id,department_id,is_active)
VALUES
(1,NEWID(),'Test Auditor','auditor@test.com',1,1,1)

SET IDENTITY_INSERT app_user OFF



/* ============================================================
QUESTION CATEGORIES
============================================================ */

SET IDENTITY_INSERT question_category ON

INSERT INTO question_category (id,name,description) VALUES
(1,'Cleanliness','Cleanliness inspection'),
(2,'Safety','Safety checks'),
(3,'Facilities','Facility condition')

SET IDENTITY_INSERT question_category OFF



/* ============================================================
QUESTION TYPES
============================================================ */

SET IDENTITY_INSERT question_type ON

INSERT INTO question_type (id,name,description) VALUES
(1,'YES_NO','Compliance yes or no'),
(2,'MULTIPLE_CHOICE','Multiple choice'),
(3,'TEXT','Text answer'),
(4,'NUMBER','Numeric answer'),
(5,'RATING','Rating scale'),
(6,'BOOLEAN','True false'),
(7,'CHECKLIST','Checklist options'),
(8,'DATE','Date value'),
(9,'TIME','Time value'),
(10,'PHOTO','Photo evidence')

SET IDENTITY_INSERT question_type OFF



/* ============================================================
QUESTION BANK
============================================================ */

SET IDENTITY_INSERT question_bank ON

INSERT INTO question_bank
(id,question_text,category_id,question_type_id,service_standard_recommendation,responsible_contractor,created_by)
VALUES

(1,'Are washroom floors clean?',1,1,'Clean every 30 minutes','Cleaning Contractor',1),

(2,'Are emergency exits accessible?',2,1,'Exit must always be clear','Security Contractor',1),

(3,'Is lighting functioning correctly?',3,1,'Lights must be operational','Maintenance Contractor',1),

(4,'Rate overall washroom cleanliness',1,5,'Minimum rating 4','Cleaning Contractor',1),

(5,'How many waste bins are available?',3,4,'Minimum 3 bins required','Maintenance Contractor',1),

(6,'Describe any visible damages',3,3,'Report damages','Maintenance Contractor',1),

(7,'Select observed safety issues',2,7,'Report hazards','Security Contractor',1),

(8,'Date of last maintenance inspection',3,8,'Monthly inspection','Maintenance Contractor',1),

(9,'Time cleaning was performed',1,9,'Every 30 minutes','Cleaning Contractor',1),

(10,'Upload photo of inspected area',1,10,'Photo evidence required','Cleaning Contractor',1)

SET IDENTITY_INSERT question_bank OFF



/* ============================================================
QUESTION OPTIONS
============================================================ */

SET IDENTITY_INSERT question_bank_option ON

INSERT INTO question_bank_option
(id,question_bank_id,option_text,option_value,display_order)
VALUES

(1,1,'YES','YES',1),
(2,1,'NO','NO',2),

(3,2,'YES','YES',1),
(4,2,'NO','NO',2),

(5,3,'YES','YES',1),
(6,3,'NO','NO',2)

SET IDENTITY_INSERT question_bank_option OFF



/* ============================================================
QUESTION REASONS
============================================================ */

SET IDENTITY_INSERT question_bank_reason ON

INSERT INTO question_bank_reason
(id,question_bank_id,reason_text)
VALUES

(1,1,'Dirty floor'),
(2,1,'Bad smell'),
(3,1,'Overflowing bins'),

(4,2,'Exit blocked'),
(5,2,'Objects blocking path'),

(6,3,'Light bulb broken'),
(7,3,'Electrical fault')

SET IDENTITY_INSERT question_bank_reason OFF



/* ============================================================
AUDIT TEMPLATE
============================================================ */

SET IDENTITY_INSERT audit_template ON

INSERT INTO audit_template
(id,name,description,version,is_active,created_by)
VALUES
(1,'Daily Airport Inspection','Standard daily inspection','1.0',1,1)

SET IDENTITY_INSERT audit_template OFF



/* ============================================================
TEMPLATE ITEMS
============================================================ */

SET IDENTITY_INSERT audit_template_item ON

INSERT INTO audit_template_item
(id,template_id,question_bank_id,sequence,mandatory)
VALUES
(1,1,1,1,1),
(2,1,2,2,1),
(3,1,3,3,1),
(4,1,4,4,0),
(5,1,5,5,0),
(6,1,6,6,0),
(7,1,7,7,0),
(8,1,8,8,0),
(9,1,9,9,0),
(10,1,10,10,0)

SET IDENTITY_INSERT audit_template_item OFF



/* ============================================================
AUDIT SITE
============================================================ */

SET IDENTITY_INSERT audit_site ON

INSERT INTO audit_site
(id,name,location,status,airport_id,department_id,created_by)
VALUES
(1,'Terminal A Washrooms','Terminal A','ACTIVE',1,1,1)

SET IDENTITY_INSERT audit_site OFF



/* ============================================================
SITE TEMPLATE LINK
============================================================ */

INSERT INTO audit_site_template
(site_id,template_id)
VALUES
(1,1)



/* ============================================================
AUDIT SCHEDULE
============================================================ */

SET IDENTITY_INSERT audit_schedule ON

INSERT INTO audit_schedule
(id,template_id,site_id,scheduler_id,scheduled_date,due_date,status)
VALUES
(1,1,1,1,GETDATE(),DATEADD(DAY,1,GETDATE()),'SCHEDULED')

SET IDENTITY_INSERT audit_schedule OFF



/* ============================================================
ASSIGN AUDITOR
============================================================ */

INSERT INTO audit_schedule_auditor
(schedule_id,auditor_id)
VALUES
(1,1)



--/* ============================================================
--AUDIT EXECUTION
--============================================================ */

--SET IDENTITY_INSERT audit_execution ON

--INSERT INTO audit_execution
--(id,schedule_id,auditor_id,status,original_audit_date)
--VALUES
--(1,1,1,'IN_PROGRESS',GETDATE())

SET IDENTITY_INSERT audit_execution OFF