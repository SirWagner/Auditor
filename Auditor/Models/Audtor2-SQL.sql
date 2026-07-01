USE [Auditor2]
GO
ALTER TABLE [question_bank_reason] DROP CONSTRAINT [FK__question___quest__6754599E]
GO
ALTER TABLE [question_bank_option] DROP CONSTRAINT [FK__question___quest__6477ECF3]
GO
ALTER TABLE [question_bank] DROP CONSTRAINT [FK__question___quest__60A75C0F]
GO
ALTER TABLE [question_bank] DROP CONSTRAINT [FK__question___creat__619B8048]
GO
ALTER TABLE [question_bank] DROP CONSTRAINT [FK__question___categ__5FB337D6]
GO
ALTER TABLE [notification] DROP CONSTRAINT [FK__notificat__recip__1AD3FDA4]
GO
ALTER TABLE [audit_template_item] DROP CONSTRAINT [FK__audit_tem__templ__6EF57B66]
GO
ALTER TABLE [audit_template_item] DROP CONSTRAINT [FK__audit_tem__quest__6FE99F9F]
GO
ALTER TABLE [audit_template] DROP CONSTRAINT [FK__audit_tem__creat__6C190EBB]
GO
ALTER TABLE [audit_site_template] DROP CONSTRAINT [FK__audit_sit__templ__7A672E12]
GO
ALTER TABLE [audit_site_template] DROP CONSTRAINT [FK__audit_sit__site___797309D9]
GO
ALTER TABLE [audit_site] DROP CONSTRAINT [FK__audit_sit__depar__75A278F5]
GO
ALTER TABLE [audit_site] DROP CONSTRAINT [FK__audit_sit__creat__76969D2E]
GO
ALTER TABLE [audit_site] DROP CONSTRAINT [FK__audit_sit__airpo__74AE54BC]
GO
ALTER TABLE [audit_schedule_auditor] DROP CONSTRAINT [FK__audit_sch__sched__03F0984C]
GO
ALTER TABLE [audit_schedule_auditor] DROP CONSTRAINT [FK__audit_sch__audit__04E4BC85]
GO
ALTER TABLE [audit_schedule] DROP CONSTRAINT [FK__audit_sch__templ__7F2BE32F]
GO
ALTER TABLE [audit_schedule] DROP CONSTRAINT [FK__audit_sch__site___00200768]
GO
ALTER TABLE [audit_schedule] DROP CONSTRAINT [FK__audit_sch__sched__01142BA1]
GO
ALTER TABLE [audit_response_reason] DROP CONSTRAINT [FK__audit_res__respo__114A936A]
GO
ALTER TABLE [audit_response_reason] DROP CONSTRAINT [FK__audit_res__reaso__123EB7A3]
GO
ALTER TABLE [audit_response_attachment] DROP CONSTRAINT [FK_response_attachment_response]
GO
ALTER TABLE [audit_response] DROP CONSTRAINT [FK__audit_res__templ__0D7A0286]
GO
ALTER TABLE [audit_response] DROP CONSTRAINT [FK__audit_res__selec__0E6E26BF]
GO
ALTER TABLE [audit_response] DROP CONSTRAINT [FK__audit_res__execu__0C85DE4D]
GO
ALTER TABLE [audit_execution_result] DROP CONSTRAINT [FK_execution_result_execution]
GO
ALTER TABLE [audit_execution] DROP CONSTRAINT [FK__audit_exe__sched__08B54D69]
GO
ALTER TABLE [audit_execution] DROP CONSTRAINT [FK__audit_exe__audit__09A971A2]
GO
ALTER TABLE [audit_attachment] DROP CONSTRAINT [FK__audit_att__execu__160F4887]
GO
ALTER TABLE [question_bank] DROP CONSTRAINT [DF__question___creat__778AC167]
GO
ALTER TABLE [question_bank] DROP CONSTRAINT [DF__question___is_ac__76969D2E]
GO
ALTER TABLE [audit_template] DROP CONSTRAINT [DF__audit_tem__creat__6A30C649]
GO
ALTER TABLE [audit_site] DROP CONSTRAINT [DF__audit_sit__creat__70DDC3D8]
GO
ALTER TABLE [audit_schedule] DROP CONSTRAINT [DF__audit_sch__creat__7D439ABD]
GO
ALTER TABLE [audit_response_attachment] DROP CONSTRAINT [DF__audit_res__uploa__245D67DE]
GO
ALTER TABLE [audit_execution_result] DROP CONSTRAINT [DF__audit_exe__calcu__1BC821DD]
GO
ALTER TABLE [audit_attachment] DROP CONSTRAINT [DF__audit_att__uploa__17F790F9]
GO
ALTER TABLE [app_user] DROP CONSTRAINT [DF__app_user__create__619B8048]
GO
ALTER TABLE [app_user] DROP CONSTRAINT [DF__app_user__is_act__60A75C0F]
GO
/****** Object:  Index [UQ__question__72E12F1B2F93D81A]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [UQ__question__72E12F1B2F93D81A] ON [question_type]
GO
/****** Object:  Index [UQ__question__72E12F1B59B99650]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [UQ__question__72E12F1B59B99650] ON [question_category]
GO
/****** Object:  Index [IX_question_bank_reason_question_bank_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_question_bank_reason_question_bank_id] ON [question_bank_reason]
GO
/****** Object:  Index [IX_question_bank_option_question_bank_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_question_bank_option_question_bank_id] ON [question_bank_option]
GO
/****** Object:  Index [IX_question_bank_question_type_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_question_bank_question_type_id] ON [question_bank]
GO
/****** Object:  Index [IX_question_bank_created_by]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_question_bank_created_by] ON [question_bank]
GO
/****** Object:  Index [idx_question_bank_category]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [idx_question_bank_category] ON [question_bank]
GO
/****** Object:  Index [idx_notification_recipient]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [idx_notification_recipient] ON [notification]
GO
/****** Object:  Index [IX_audit_template_item_question_bank_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_template_item_question_bank_id] ON [audit_template_item]
GO
/****** Object:  Index [idx_template_item_template]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [idx_template_item_template] ON [audit_template_item]
GO
/****** Object:  Index [UQ__audit_te__72E12F1B84BFD71C]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [UQ__audit_te__72E12F1B84BFD71C] ON [audit_template]
GO
/****** Object:  Index [IX_audit_template_created_by]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_template_created_by] ON [audit_template]
GO
/****** Object:  Index [IX_audit_site_template_template_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_site_template_template_id] ON [audit_site_template]
GO
/****** Object:  Index [IX_audit_site_department_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_site_department_id] ON [audit_site]
GO
/****** Object:  Index [IX_audit_site_created_by]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_site_created_by] ON [audit_site]
GO
/****** Object:  Index [IX_audit_site_airport_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_site_airport_id] ON [audit_site]
GO
/****** Object:  Index [IX_audit_schedule_auditor_auditor_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_schedule_auditor_auditor_id] ON [audit_schedule_auditor]
GO
/****** Object:  Index [IX_audit_schedule_template_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_schedule_template_id] ON [audit_schedule]
GO
/****** Object:  Index [IX_audit_schedule_site_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_schedule_site_id] ON [audit_schedule]
GO
/****** Object:  Index [IX_audit_schedule_scheduler_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_schedule_scheduler_id] ON [audit_schedule]
GO
/****** Object:  Index [idx_schedule_status]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [idx_schedule_status] ON [audit_schedule]
GO
/****** Object:  Index [IX_audit_response_reason_reason_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_response_reason_reason_id] ON [audit_response_reason]
GO
/****** Object:  Index [IX_audit_response_attachment_response_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_response_attachment_response_id] ON [audit_response_attachment]
GO
/****** Object:  Index [IX_audit_response_template_item_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_response_template_item_id] ON [audit_response]
GO
/****** Object:  Index [IX_audit_response_selected_option_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_response_selected_option_id] ON [audit_response]
GO
/****** Object:  Index [idx_response_execution]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [idx_response_execution] ON [audit_response]
GO
/****** Object:  Index [UQ_execution_result_execution]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [UQ_execution_result_execution] ON [audit_execution_result]
GO
/****** Object:  Index [IX_audit_execution_schedule_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_execution_schedule_id] ON [audit_execution]
GO
/****** Object:  Index [IX_audit_execution_auditor_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_execution_auditor_id] ON [audit_execution]
GO
/****** Object:  Index [idx_execution_status]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [idx_execution_status] ON [audit_execution]
GO
/****** Object:  Index [IX_audit_attachment_execution_id]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [IX_audit_attachment_execution_id] ON [audit_attachment]
GO
/****** Object:  Index [UQ__app_user__0E42BF72C6C8469F]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [UQ__app_user__0E42BF72C6C8469F] ON [app_user]
GO
/****** Object:  Index [UQ__airport__72E12F1B1E2BB23C]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [UQ__airport__72E12F1B1E2BB23C] ON [airport]
GO
/****** Object:  Index [UQ__airport__357D4CF90A3E0150]    Script Date: 01/07/2026 20:29:37 ******/
DROP INDEX [UQ__airport__357D4CF90A3E0150] ON [airport]
GO
/****** Object:  Table [question_type]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[question_type]') AND type in (N'U'))
DROP TABLE [question_type]
GO
/****** Object:  Table [question_category]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[question_category]') AND type in (N'U'))
DROP TABLE [question_category]
GO
/****** Object:  Table [question_bank_reason]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[question_bank_reason]') AND type in (N'U'))
DROP TABLE [question_bank_reason]
GO
/****** Object:  Table [question_bank_option]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[question_bank_option]') AND type in (N'U'))
DROP TABLE [question_bank_option]
GO
/****** Object:  Table [question_bank]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[question_bank]') AND type in (N'U'))
DROP TABLE [question_bank]
GO
/****** Object:  Table [notification]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[notification]') AND type in (N'U'))
DROP TABLE [notification]
GO
/****** Object:  Table [department]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[department]') AND type in (N'U'))
DROP TABLE [department]
GO
/****** Object:  Table [audit_template_item]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_template_item]') AND type in (N'U'))
DROP TABLE [audit_template_item]
GO
/****** Object:  Table [audit_template]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_template]') AND type in (N'U'))
DROP TABLE [audit_template]
GO
/****** Object:  Table [audit_site_template]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_site_template]') AND type in (N'U'))
DROP TABLE [audit_site_template]
GO
/****** Object:  Table [audit_site]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_site]') AND type in (N'U'))
DROP TABLE [audit_site]
GO
/****** Object:  Table [audit_schedule_auditor]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_schedule_auditor]') AND type in (N'U'))
DROP TABLE [audit_schedule_auditor]
GO
/****** Object:  Table [audit_schedule]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_schedule]') AND type in (N'U'))
DROP TABLE [audit_schedule]
GO
/****** Object:  Table [audit_response_reason]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_response_reason]') AND type in (N'U'))
DROP TABLE [audit_response_reason]
GO
/****** Object:  Table [audit_response_attachment]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_response_attachment]') AND type in (N'U'))
DROP TABLE [audit_response_attachment]
GO
/****** Object:  Table [audit_response]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_response]') AND type in (N'U'))
DROP TABLE [audit_response]
GO
/****** Object:  Table [audit_execution_result]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_execution_result]') AND type in (N'U'))
DROP TABLE [audit_execution_result]
GO
/****** Object:  Table [audit_execution]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_execution]') AND type in (N'U'))
DROP TABLE [audit_execution]
GO
/****** Object:  Table [audit_attachment]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[audit_attachment]') AND type in (N'U'))
DROP TABLE [audit_attachment]
GO
/****** Object:  Table [app_user]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[app_user]') AND type in (N'U'))
DROP TABLE [app_user]
GO
/****** Object:  Table [airport]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[airport]') AND type in (N'U'))
DROP TABLE [airport]
GO
/****** Object:  Table [__EFMigrationsHistory]    Script Date: 01/07/2026 20:29:37 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[__EFMigrationsHistory]') AND type in (N'U'))
DROP TABLE [__EFMigrationsHistory]
GO
USE [master]
GO
/****** Object:  Database [Auditor2]    Script Date: 01/07/2026 20:29:37 ******/
DROP DATABASE [Auditor2]
GO
/****** Object:  Database [Auditor2]    Script Date: 01/07/2026 20:29:37 ******/
CREATE DATABASE [Auditor2]
GO
ALTER DATABASE [Auditor2] SET COMPATIBILITY_LEVEL = 170
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Auditor2].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Auditor2] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Auditor2] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Auditor2] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Auditor2] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Auditor2] SET ARITHABORT OFF 
GO
ALTER DATABASE [Auditor2] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [Auditor2] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Auditor2] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Auditor2] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Auditor2] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Auditor2] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Auditor2] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Auditor2] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Auditor2] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Auditor2] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Auditor2] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Auditor2] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Auditor2] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Auditor2] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Auditor2] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Auditor2] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [Auditor2] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Auditor2] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Auditor2] SET  MULTI_USER 
GO
ALTER DATABASE [Auditor2] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Auditor2] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Auditor2] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Auditor2] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Auditor2] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Auditor2] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [Auditor2] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [Auditor2] SET QUERY_STORE = ON
GO
USE [Auditor2]
GO
/****** Object:  Table [__EFMigrationsHistory]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [airport]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [airport](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[code] [nvarchar](50) NOT NULL,
	[location] [nvarchar](255) NULL,
	[AirSide] [nvarchar](max) NULL,
	[ComercialArea] [nvarchar](max) NULL,
	[TerminalSection] [nvarchar](max) NULL,
 CONSTRAINT [PK__airport__3213E83F178E14CD] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [app_user]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app_user](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[azure_ad_object_id] [uniqueidentifier] NOT NULL,
	[password_hash] [nvarchar](255) NULL,
	[display_name] [nvarchar](255) NULL,
	[email] [nvarchar](255) NULL,
	[is_active] [bit] NOT NULL,
	[created_date] [datetime2](7) NOT NULL,
	[role] [nvarchar](50) NULL,
 CONSTRAINT [PK__app_user__3213E83F1159EBF7] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_attachment]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_attachment](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[execution_id] [bigint] NOT NULL,
	[file_name] [nvarchar](255) NOT NULL,
	[file_url] [nvarchar](500) NULL,
	[content_type] [nvarchar](100) NULL,
	[file_size] [bigint] NULL,
	[uploaded_date] [datetime2](7) NULL,
 CONSTRAINT [PK__audit_at__3213E83F3A4458CE] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_execution]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_execution](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[schedule_id] [bigint] NOT NULL,
	[auditor_id] [bigint] NOT NULL,
	[status] [varchar](50) NOT NULL,
	[acceptance_date] [datetime2](7) NULL,
	[rejection_reason] [nvarchar](2000) NULL,
	[submission_date] [datetime2](7) NULL,
	[original_audit_date] [datetime2](7) NOT NULL,
 CONSTRAINT [PK__audit_ex__3213E83FA1316016] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_execution_result]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_execution_result](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[execution_id] [bigint] NOT NULL,
	[total_questions] [int] NOT NULL,
	[compliant_count] [int] NOT NULL,
	[non_compliant_count] [int] NOT NULL,
	[not_applicable_count] [int] NULL,
	[score_percentage] [decimal](5, 2) NOT NULL,
	[calculated_date] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_audit_execution_result] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_response]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_response](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[execution_id] [bigint] NOT NULL,
	[template_item_id] [bigint] NOT NULL,
	[selected_option_id] [bigint] NULL,
	[comment] [nvarchar](2000) NULL,
	[compliant] [bit] NULL,
 CONSTRAINT [PK__audit_re__3213E83F55E5FE9F] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_response_attachment]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_response_attachment](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[response_id] [bigint] NOT NULL,
	[file_name] [nvarchar](255) NOT NULL,
	[file_url] [nvarchar](500) NULL,
	[content_type] [nvarchar](100) NULL,
	[file_size] [bigint] NULL,
	[uploaded_date] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_audit_response_attachment] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_response_reason]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_response_reason](
	[response_id] [bigint] NOT NULL,
	[reason_id] [bigint] NOT NULL,
 CONSTRAINT [PK__audit_re__B3AA63C32753E0CD] PRIMARY KEY CLUSTERED 
(
	[response_id] ASC,
	[reason_id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_schedule]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_schedule](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[template_id] [bigint] NOT NULL,
	[site_id] [bigint] NOT NULL,
	[scheduler_id] [bigint] NOT NULL,
	[scheduled_date] [datetime2](7) NOT NULL,
	[due_date] [datetime2](7) NOT NULL,
	[status] [varchar](50) NOT NULL,
	[modification_reason] [nvarchar](1000) NULL,
	[cancellation_reason] [nvarchar](1000) NULL,
	[created_date] [datetime2](7) NOT NULL,
 CONSTRAINT [PK__audit_sc__3213E83F48C5834A] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_schedule_auditor]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_schedule_auditor](
	[schedule_id] [bigint] NOT NULL,
	[auditor_id] [bigint] NOT NULL,
 CONSTRAINT [PK__audit_sc__BD9E29F05A8F4534] PRIMARY KEY CLUSTERED 
(
	[schedule_id] ASC,
	[auditor_id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_site]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_site](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[location] [nvarchar](255) NULL,
	[status] [varchar](50) NOT NULL,
	[airport_id] [bigint] NOT NULL,
	[department_id] [bigint] NOT NULL,
	[created_by] [bigint] NOT NULL,
	[created_date] [datetime2](7) NOT NULL,
 CONSTRAINT [PK__audit_si__3213E83F2E69B5A2] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_site_template]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_site_template](
	[site_id] [bigint] NOT NULL,
	[template_id] [bigint] NOT NULL,
 CONSTRAINT [PK__audit_si__39CB95CD4E5145E5] PRIMARY KEY CLUSTERED 
(
	[site_id] ASC,
	[template_id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_template]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_template](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](1000) NULL,
	[version] [nvarchar](50) NOT NULL,
	[is_active] [bit] NOT NULL,
	[created_by] [bigint] NOT NULL,
	[created_date] [datetime2](7) NOT NULL,
 CONSTRAINT [PK__audit_te__3213E83FB8A2B4CB] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [audit_template_item]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [audit_template_item](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[template_id] [bigint] NOT NULL,
	[question_bank_id] [bigint] NOT NULL,
	[sequence] [int] NOT NULL,
	[mandatory] [bit] NOT NULL,
 CONSTRAINT [PK__audit_te__3213E83F30483F4F] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [department]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [department](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](500) NULL,
 CONSTRAINT [PK__departme__3213E83FADB797C3] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [notification]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [notification](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[recipient_id] [bigint] NOT NULL,
	[subject] [nvarchar](255) NOT NULL,
	[message] [nvarchar](max) NOT NULL,
	[type] [varchar](50) NOT NULL,
	[entity_type] [nvarchar](100) NULL,
	[entity_id] [bigint] NULL,
	[sent_date] [datetime2](7) NULL,
	[is_read] [bit] NOT NULL,
 CONSTRAINT [PK__notifica__3213E83FAD279980] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [question_bank]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [question_bank](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[question_text] [nvarchar](2000) NOT NULL,
	[category_id] [bigint] NOT NULL,
	[question_type_id] [bigint] NOT NULL,
	[service_standard_recommendation] [nvarchar](2000) NULL,
	[responsible_contractor] [nvarchar](255) NULL,
	[is_active] [bit] NOT NULL,
	[created_by] [bigint] NOT NULL,
	[created_date] [datetime2](7) NOT NULL,
 CONSTRAINT [PK__question__3213E83FD16E9283] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [question_bank_option]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [question_bank_option](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[question_bank_id] [bigint] NOT NULL,
	[option_text] [nvarchar](500) NOT NULL,
	[option_value] [nvarchar](100) NULL,
	[display_order] [int] NOT NULL,
	[requires_reason] [bit] NOT NULL,
 CONSTRAINT [PK__question__3213E83F9030CCB8] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [question_bank_reason]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [question_bank_reason](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[question_bank_id] [bigint] NOT NULL,
	[reason_text] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK__question__3213E83F6ED53A41] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [question_category]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [question_category](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](500) NULL,
 CONSTRAINT [PK__question__3213E83F12F8191F] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [question_type]    Script Date: 01/07/2026 20:29:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [question_type](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[description] [nvarchar](500) NULL,
 CONSTRAINT [PK__question__3213E83F70F1D122] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260525135907_AddPasswordHashToAppUser', N'8.0.23')
GO
INSERT [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260526164539_AddFieldsToAirport', N'8.0.23')
GO
SET IDENTITY_INSERT [airport] ON 
GO
INSERT [airport] ([id], [name], [code], [location], [AirSide], [ComercialArea], [TerminalSection]) VALUES (1, N'OR Tambo International', N'ORT', N'Johannesburg', NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [airport] OFF
GO
SET IDENTITY_INSERT [app_user] ON 
GO
INSERT [app_user] ([id], [azure_ad_object_id], [password_hash], [display_name], [email], [is_active], [created_date], [role]) VALUES (1, N'02e76903-f7da-4327-bcb3-9d59adb52c39', NULL, N'Test Auditor', N'auditor@test.com', 1, CAST(N'2026-06-21T21:17:48.8433851' AS DateTime2), NULL)
GO
INSERT [app_user] ([id], [azure_ad_object_id], [password_hash], [display_name], [email], [is_active], [created_date], [role]) VALUES (2, N'4a332ebd-2317-42af-9855-87d8ce983258', N'73l8gRjwLftklgfdXT+MdiMEjJwGPVMsyVxe16iYpk8=', N'Mapie', N'm@g', 1, CAST(N'2026-06-26T12:34:00.6336161' AS DateTime2), N'Admin')
GO
SET IDENTITY_INSERT [app_user] OFF
GO
SET IDENTITY_INSERT [audit_schedule] ON 
GO
INSERT [audit_schedule] ([id], [template_id], [site_id], [scheduler_id], [scheduled_date], [due_date], [status], [modification_reason], [cancellation_reason], [created_date]) VALUES (1, 1, 1, 1, CAST(N'2026-06-21T21:19:34.9300000' AS DateTime2), CAST(N'2026-06-22T21:19:34.9300000' AS DateTime2), N'SCHEDULED', NULL, NULL, CAST(N'2026-06-21T21:19:34.9320155' AS DateTime2))
GO
SET IDENTITY_INSERT [audit_schedule] OFF
GO
INSERT [audit_schedule_auditor] ([schedule_id], [auditor_id]) VALUES (1, 1)
GO
SET IDENTITY_INSERT [audit_site] ON 
GO
INSERT [audit_site] ([id], [name], [location], [status], [airport_id], [department_id], [created_by], [created_date]) VALUES (1, N'Terminal A Washrooms', N'Terminal A', N'ACTIVE', 1, 1, 1, CAST(N'2026-06-21T21:19:27.7442793' AS DateTime2))
GO
SET IDENTITY_INSERT [audit_site] OFF
GO
INSERT [audit_site_template] ([site_id], [template_id]) VALUES (1, 1)
GO
SET IDENTITY_INSERT [audit_template] ON 
GO
INSERT [audit_template] ([id], [name], [description], [version], [is_active], [created_by], [created_date]) VALUES (1, N'Daily Airport Inspection', N'Standard daily inspection', N'1.0', 1, 1, CAST(N'2026-06-21T21:19:19.4255440' AS DateTime2))
GO
SET IDENTITY_INSERT [audit_template] OFF
GO
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
SET IDENTITY_INSERT [department] ON 
GO
INSERT [department] ([id], [name], [description]) VALUES (1, N'Operations', N'Airport Operations')
GO
SET IDENTITY_INSERT [department] OFF
GO
SET IDENTITY_INSERT [question_bank] ON 
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (1, N'Are washroom floors clean?', 1, 1, N'Clean every 30 minutes', N'Cleaning Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (2, N'Are emergency exits accessible?', 2, 1, N'Exit must always be clear', N'Security Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (3, N'Is lighting functioning correctly?', 3, 1, N'Lights must be operational', N'Maintenance Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (4, N'Rate overall washroom cleanliness', 1, 5, N'Minimum rating 4', N'Cleaning Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (5, N'How many waste bins are available?', 3, 4, N'Minimum 3 bins required', N'Maintenance Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (6, N'Describe any visible damages', 3, 3, N'Report damages', N'Maintenance Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (7, N'Select observed safety issues', 2, 7, N'Report hazards', N'Security Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (8, N'Date of last maintenance inspection', 3, 8, N'Monthly inspection', N'Maintenance Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (9, N'Time cleaning was performed', 1, 9, N'Every 30 minutes', N'Cleaning Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (10, N'Upload photo of inspected area', 1, 10, N'Photo evidence required', N'Cleaning Contractor', 1, 1, CAST(N'2026-06-21T21:18:01.4202747' AS DateTime2))
GO
INSERT [question_bank] ([id], [question_text], [category_id], [question_type_id], [service_standard_recommendation], [responsible_contractor], [is_active], [created_by], [created_date]) VALUES (11, N'asdasd', 1, 1, N'123', N'12', 0, 1, CAST(N'2026-06-21T21:20:11.9718846' AS DateTime2))
GO
SET IDENTITY_INSERT [question_bank] OFF
GO
SET IDENTITY_INSERT [question_bank_option] ON 
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (1, 1, N'YES', N'YES', 1, 0)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (2, 1, N'NO', N'NO', 2, 0)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (3, 2, N'YES', N'YES', 1, 0)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (4, 2, N'NO', N'NO', 2, 0)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (5, 3, N'YES', N'YES', 1, 0)
GO
INSERT [question_bank_option] ([id], [question_bank_id], [option_text], [option_value], [display_order], [requires_reason]) VALUES (6, 3, N'NO', N'NO', 2, 0)
GO
SET IDENTITY_INSERT [question_bank_option] OFF
GO
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
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__airport__357D4CF90A3E0150]    Script Date: 01/07/2026 20:29:37 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ__airport__357D4CF90A3E0150] ON [airport]
(
	[code] ASC
)
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__airport__72E12F1B1E2BB23C]    Script Date: 01/07/2026 20:29:37 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ__airport__72E12F1B1E2BB23C] ON [airport]
(
	[name] ASC
)
GO
/****** Object:  Index [UQ__app_user__0E42BF72C6C8469F]    Script Date: 01/07/2026 20:29:37 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ__app_user__0E42BF72C6C8469F] ON [app_user]
(
	[azure_ad_object_id] ASC
)
GO
/****** Object:  Index [IX_audit_attachment_execution_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_attachment_execution_id] ON [audit_attachment]
(
	[execution_id] ASC
)
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_execution_status]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [idx_execution_status] ON [audit_execution]
(
	[status] ASC
)
GO
/****** Object:  Index [IX_audit_execution_auditor_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_execution_auditor_id] ON [audit_execution]
(
	[auditor_id] ASC
)
GO
/****** Object:  Index [IX_audit_execution_schedule_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_execution_schedule_id] ON [audit_execution]
(
	[schedule_id] ASC
)
GO
/****** Object:  Index [UQ_execution_result_execution]    Script Date: 01/07/2026 20:29:37 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_execution_result_execution] ON [audit_execution_result]
(
	[execution_id] ASC
)
GO
/****** Object:  Index [idx_response_execution]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [idx_response_execution] ON [audit_response]
(
	[execution_id] ASC
)
GO
/****** Object:  Index [IX_audit_response_selected_option_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_response_selected_option_id] ON [audit_response]
(
	[selected_option_id] ASC
)
GO
/****** Object:  Index [IX_audit_response_template_item_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_response_template_item_id] ON [audit_response]
(
	[template_item_id] ASC
)
GO
/****** Object:  Index [IX_audit_response_attachment_response_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_response_attachment_response_id] ON [audit_response_attachment]
(
	[response_id] ASC
)
GO
/****** Object:  Index [IX_audit_response_reason_reason_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_response_reason_reason_id] ON [audit_response_reason]
(
	[reason_id] ASC
)
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_schedule_status]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [idx_schedule_status] ON [audit_schedule]
(
	[status] ASC
)
GO
/****** Object:  Index [IX_audit_schedule_scheduler_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_schedule_scheduler_id] ON [audit_schedule]
(
	[scheduler_id] ASC
)
GO
/****** Object:  Index [IX_audit_schedule_site_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_schedule_site_id] ON [audit_schedule]
(
	[site_id] ASC
)
GO
/****** Object:  Index [IX_audit_schedule_template_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_schedule_template_id] ON [audit_schedule]
(
	[template_id] ASC
)
GO
/****** Object:  Index [IX_audit_schedule_auditor_auditor_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_schedule_auditor_auditor_id] ON [audit_schedule_auditor]
(
	[auditor_id] ASC
)
GO
/****** Object:  Index [IX_audit_site_airport_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_site_airport_id] ON [audit_site]
(
	[airport_id] ASC
)
GO
/****** Object:  Index [IX_audit_site_created_by]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_site_created_by] ON [audit_site]
(
	[created_by] ASC
)
GO
/****** Object:  Index [IX_audit_site_department_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_site_department_id] ON [audit_site]
(
	[department_id] ASC
)
GO
/****** Object:  Index [IX_audit_site_template_template_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_site_template_template_id] ON [audit_site_template]
(
	[template_id] ASC
)
GO
/****** Object:  Index [IX_audit_template_created_by]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_template_created_by] ON [audit_template]
(
	[created_by] ASC
)
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__audit_te__72E12F1B84BFD71C]    Script Date: 01/07/2026 20:29:37 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ__audit_te__72E12F1B84BFD71C] ON [audit_template]
(
	[name] ASC
)
GO
/****** Object:  Index [idx_template_item_template]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [idx_template_item_template] ON [audit_template_item]
(
	[template_id] ASC
)
GO
/****** Object:  Index [IX_audit_template_item_question_bank_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_audit_template_item_question_bank_id] ON [audit_template_item]
(
	[question_bank_id] ASC
)
GO
/****** Object:  Index [idx_notification_recipient]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [idx_notification_recipient] ON [notification]
(
	[recipient_id] ASC
)
GO
/****** Object:  Index [idx_question_bank_category]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [idx_question_bank_category] ON [question_bank]
(
	[category_id] ASC
)
GO
/****** Object:  Index [IX_question_bank_created_by]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_question_bank_created_by] ON [question_bank]
(
	[created_by] ASC
)
GO
/****** Object:  Index [IX_question_bank_question_type_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_question_bank_question_type_id] ON [question_bank]
(
	[question_type_id] ASC
)
GO
/****** Object:  Index [IX_question_bank_option_question_bank_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_question_bank_option_question_bank_id] ON [question_bank_option]
(
	[question_bank_id] ASC
)
GO
/****** Object:  Index [IX_question_bank_reason_question_bank_id]    Script Date: 01/07/2026 20:29:37 ******/
CREATE NONCLUSTERED INDEX [IX_question_bank_reason_question_bank_id] ON [question_bank_reason]
(
	[question_bank_id] ASC
)
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__question__72E12F1B59B99650]    Script Date: 01/07/2026 20:29:37 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ__question__72E12F1B59B99650] ON [question_category]
(
	[name] ASC
)
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__question__72E12F1B2F93D81A]    Script Date: 01/07/2026 20:29:37 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ__question__72E12F1B2F93D81A] ON [question_type]
(
	[name] ASC
)
GO
ALTER TABLE [app_user] ADD  DEFAULT (CONVERT([bit],(1))) FOR [is_active]
GO
ALTER TABLE [app_user] ADD  DEFAULT (sysdatetime()) FOR [created_date]
GO
ALTER TABLE [audit_attachment] ADD  DEFAULT (sysdatetime()) FOR [uploaded_date]
GO
ALTER TABLE [audit_execution_result] ADD  DEFAULT (sysdatetime()) FOR [calculated_date]
GO
ALTER TABLE [audit_response_attachment] ADD  DEFAULT (sysdatetime()) FOR [uploaded_date]
GO
ALTER TABLE [audit_schedule] ADD  DEFAULT (sysdatetime()) FOR [created_date]
GO
ALTER TABLE [audit_site] ADD  DEFAULT (sysdatetime()) FOR [created_date]
GO
ALTER TABLE [audit_template] ADD  DEFAULT (sysdatetime()) FOR [created_date]
GO
ALTER TABLE [question_bank] ADD  DEFAULT (CONVERT([bit],(1))) FOR [is_active]
GO
ALTER TABLE [question_bank] ADD  DEFAULT (sysdatetime()) FOR [created_date]
GO
ALTER TABLE [audit_attachment]  WITH CHECK ADD  CONSTRAINT [FK__audit_att__execu__160F4887] FOREIGN KEY([execution_id])
REFERENCES [audit_execution] ([id])
GO
ALTER TABLE [audit_attachment] CHECK CONSTRAINT [FK__audit_att__execu__160F4887]
GO
ALTER TABLE [audit_execution]  WITH CHECK ADD  CONSTRAINT [FK__audit_exe__audit__09A971A2] FOREIGN KEY([auditor_id])
REFERENCES [app_user] ([id])
GO
ALTER TABLE [audit_execution] CHECK CONSTRAINT [FK__audit_exe__audit__09A971A2]
GO
ALTER TABLE [audit_execution]  WITH CHECK ADD  CONSTRAINT [FK__audit_exe__sched__08B54D69] FOREIGN KEY([schedule_id])
REFERENCES [audit_schedule] ([id])
GO
ALTER TABLE [audit_execution] CHECK CONSTRAINT [FK__audit_exe__sched__08B54D69]
GO
ALTER TABLE [audit_execution_result]  WITH CHECK ADD  CONSTRAINT [FK_execution_result_execution] FOREIGN KEY([execution_id])
REFERENCES [audit_execution] ([id])
GO
ALTER TABLE [audit_execution_result] CHECK CONSTRAINT [FK_execution_result_execution]
GO
ALTER TABLE [audit_response]  WITH CHECK ADD  CONSTRAINT [FK__audit_res__execu__0C85DE4D] FOREIGN KEY([execution_id])
REFERENCES [audit_execution] ([id])
GO
ALTER TABLE [audit_response] CHECK CONSTRAINT [FK__audit_res__execu__0C85DE4D]
GO
ALTER TABLE [audit_response]  WITH CHECK ADD  CONSTRAINT [FK__audit_res__selec__0E6E26BF] FOREIGN KEY([selected_option_id])
REFERENCES [question_bank_option] ([id])
GO
ALTER TABLE [audit_response] CHECK CONSTRAINT [FK__audit_res__selec__0E6E26BF]
GO
ALTER TABLE [audit_response]  WITH CHECK ADD  CONSTRAINT [FK__audit_res__templ__0D7A0286] FOREIGN KEY([template_item_id])
REFERENCES [audit_template_item] ([id])
GO
ALTER TABLE [audit_response] CHECK CONSTRAINT [FK__audit_res__templ__0D7A0286]
GO
ALTER TABLE [audit_response_attachment]  WITH CHECK ADD  CONSTRAINT [FK_response_attachment_response] FOREIGN KEY([response_id])
REFERENCES [audit_response] ([id])
GO
ALTER TABLE [audit_response_attachment] CHECK CONSTRAINT [FK_response_attachment_response]
GO
ALTER TABLE [audit_response_reason]  WITH CHECK ADD  CONSTRAINT [FK__audit_res__reaso__123EB7A3] FOREIGN KEY([reason_id])
REFERENCES [question_bank_reason] ([id])
GO
ALTER TABLE [audit_response_reason] CHECK CONSTRAINT [FK__audit_res__reaso__123EB7A3]
GO
ALTER TABLE [audit_response_reason]  WITH CHECK ADD  CONSTRAINT [FK__audit_res__respo__114A936A] FOREIGN KEY([response_id])
REFERENCES [audit_response] ([id])
GO
ALTER TABLE [audit_response_reason] CHECK CONSTRAINT [FK__audit_res__respo__114A936A]
GO
ALTER TABLE [audit_schedule]  WITH CHECK ADD  CONSTRAINT [FK__audit_sch__sched__01142BA1] FOREIGN KEY([scheduler_id])
REFERENCES [app_user] ([id])
GO
ALTER TABLE [audit_schedule] CHECK CONSTRAINT [FK__audit_sch__sched__01142BA1]
GO
ALTER TABLE [audit_schedule]  WITH CHECK ADD  CONSTRAINT [FK__audit_sch__site___00200768] FOREIGN KEY([site_id])
REFERENCES [audit_site] ([id])
GO
ALTER TABLE [audit_schedule] CHECK CONSTRAINT [FK__audit_sch__site___00200768]
GO
ALTER TABLE [audit_schedule]  WITH CHECK ADD  CONSTRAINT [FK__audit_sch__templ__7F2BE32F] FOREIGN KEY([template_id])
REFERENCES [audit_template] ([id])
GO
ALTER TABLE [audit_schedule] CHECK CONSTRAINT [FK__audit_sch__templ__7F2BE32F]
GO
ALTER TABLE [audit_schedule_auditor]  WITH CHECK ADD  CONSTRAINT [FK__audit_sch__audit__04E4BC85] FOREIGN KEY([auditor_id])
REFERENCES [app_user] ([id])
GO
ALTER TABLE [audit_schedule_auditor] CHECK CONSTRAINT [FK__audit_sch__audit__04E4BC85]
GO
ALTER TABLE [audit_schedule_auditor]  WITH CHECK ADD  CONSTRAINT [FK__audit_sch__sched__03F0984C] FOREIGN KEY([schedule_id])
REFERENCES [audit_schedule] ([id])
GO
ALTER TABLE [audit_schedule_auditor] CHECK CONSTRAINT [FK__audit_sch__sched__03F0984C]
GO
ALTER TABLE [audit_site]  WITH CHECK ADD  CONSTRAINT [FK__audit_sit__airpo__74AE54BC] FOREIGN KEY([airport_id])
REFERENCES [airport] ([id])
GO
ALTER TABLE [audit_site] CHECK CONSTRAINT [FK__audit_sit__airpo__74AE54BC]
GO
ALTER TABLE [audit_site]  WITH CHECK ADD  CONSTRAINT [FK__audit_sit__creat__76969D2E] FOREIGN KEY([created_by])
REFERENCES [app_user] ([id])
GO
ALTER TABLE [audit_site] CHECK CONSTRAINT [FK__audit_sit__creat__76969D2E]
GO
ALTER TABLE [audit_site]  WITH CHECK ADD  CONSTRAINT [FK__audit_sit__depar__75A278F5] FOREIGN KEY([department_id])
REFERENCES [department] ([id])
GO
ALTER TABLE [audit_site] CHECK CONSTRAINT [FK__audit_sit__depar__75A278F5]
GO
ALTER TABLE [audit_site_template]  WITH CHECK ADD  CONSTRAINT [FK__audit_sit__site___797309D9] FOREIGN KEY([site_id])
REFERENCES [audit_site] ([id])
GO
ALTER TABLE [audit_site_template] CHECK CONSTRAINT [FK__audit_sit__site___797309D9]
GO
ALTER TABLE [audit_site_template]  WITH CHECK ADD  CONSTRAINT [FK__audit_sit__templ__7A672E12] FOREIGN KEY([template_id])
REFERENCES [audit_template] ([id])
GO
ALTER TABLE [audit_site_template] CHECK CONSTRAINT [FK__audit_sit__templ__7A672E12]
GO
ALTER TABLE [audit_template]  WITH CHECK ADD  CONSTRAINT [FK__audit_tem__creat__6C190EBB] FOREIGN KEY([created_by])
REFERENCES [app_user] ([id])
GO
ALTER TABLE [audit_template] CHECK CONSTRAINT [FK__audit_tem__creat__6C190EBB]
GO
ALTER TABLE [audit_template_item]  WITH CHECK ADD  CONSTRAINT [FK__audit_tem__quest__6FE99F9F] FOREIGN KEY([question_bank_id])
REFERENCES [question_bank] ([id])
GO
ALTER TABLE [audit_template_item] CHECK CONSTRAINT [FK__audit_tem__quest__6FE99F9F]
GO
ALTER TABLE [audit_template_item]  WITH CHECK ADD  CONSTRAINT [FK__audit_tem__templ__6EF57B66] FOREIGN KEY([template_id])
REFERENCES [audit_template] ([id])
GO
ALTER TABLE [audit_template_item] CHECK CONSTRAINT [FK__audit_tem__templ__6EF57B66]
GO
ALTER TABLE [notification]  WITH CHECK ADD  CONSTRAINT [FK__notificat__recip__1AD3FDA4] FOREIGN KEY([recipient_id])
REFERENCES [app_user] ([id])
GO
ALTER TABLE [notification] CHECK CONSTRAINT [FK__notificat__recip__1AD3FDA4]
GO
ALTER TABLE [question_bank]  WITH CHECK ADD  CONSTRAINT [FK__question___categ__5FB337D6] FOREIGN KEY([category_id])
REFERENCES [question_category] ([id])
GO
ALTER TABLE [question_bank] CHECK CONSTRAINT [FK__question___categ__5FB337D6]
GO
ALTER TABLE [question_bank]  WITH CHECK ADD  CONSTRAINT [FK__question___creat__619B8048] FOREIGN KEY([created_by])
REFERENCES [app_user] ([id])
GO
ALTER TABLE [question_bank] CHECK CONSTRAINT [FK__question___creat__619B8048]
GO
ALTER TABLE [question_bank]  WITH CHECK ADD  CONSTRAINT [FK__question___quest__60A75C0F] FOREIGN KEY([question_type_id])
REFERENCES [question_type] ([id])
GO
ALTER TABLE [question_bank] CHECK CONSTRAINT [FK__question___quest__60A75C0F]
GO
ALTER TABLE [question_bank_option]  WITH CHECK ADD  CONSTRAINT [FK__question___quest__6477ECF3] FOREIGN KEY([question_bank_id])
REFERENCES [question_bank] ([id])
GO
ALTER TABLE [question_bank_option] CHECK CONSTRAINT [FK__question___quest__6477ECF3]
GO
ALTER TABLE [question_bank_reason]  WITH CHECK ADD  CONSTRAINT [FK__question___quest__6754599E] FOREIGN KEY([question_bank_id])
REFERENCES [question_bank] ([id])
GO
ALTER TABLE [question_bank_reason] CHECK CONSTRAINT [FK__question___quest__6754599E]
GO
USE [master]
GO
ALTER DATABASE [Auditor2] SET  READ_WRITE 
GO
