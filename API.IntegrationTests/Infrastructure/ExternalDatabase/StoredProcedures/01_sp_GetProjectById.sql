CREATE PROCEDURE [dbo].[GetProjectById]
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id,
		Name,
		DepartmentName,
		IsActive,
		CreatedAt
	FROM Projects
	WHERE Id = @Id
END