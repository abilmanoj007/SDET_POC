CREATE PROCEDURE [dbo].[InsertAuditLog]
	@Id UNIQUEIDENTIFIER,
	@Action VARCHAR(300),
	@PerformedBy VARCHAR(200)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO AuditLog (Id, Action, PerformedBy)
	VALUES (@Id, @Action, @PerformedBy)
END