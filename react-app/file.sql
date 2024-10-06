CREATE TABLE [dbo].[Employee](
[EmployeeId] [int] IDENTITY(1,1) NOT NULL,
[FirstName] [varchar](50) NOT NULL,
[LastName] [varchar](50) NOT NULL,
[ReportsTo] [int] NULL,
[DepartmentId] [int] NULL,
[BirthDate] [datetime] NULL,
[HireDate] [datetime] NOT NULL,
[TerminationDate] [datetime] NULL,
CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED ([EmployeeId] ASC)
)
CREATE TABLE [dbo].[Department](
[DepartmentId] [int] IDENTITY(1,1) NOT NULL,
[DepartmentName] [varchar](50) NOT NULL,
[CostCenter] [varchar](50) NULL,
[LocationId] [int] NULL,
CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED ([DepartmentId] ASC)
)
ALTER TABLE [dbo].[Employee] WITH CHECK ADD CONSTRAINT
[FK_Employee_Department] FOREIGN KEY([DepartmentId])
REFERENCES [dbo].[Department] ([DepartmentId])