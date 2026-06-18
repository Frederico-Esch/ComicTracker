# Next

1. Filter by status
2. Save dates (Update the db)
3. Save file types for comics (Update the db)
4. Update db scheme rebuilding in code
5. Disable auto_vacuum in db
6. Vacuum only when closing application (`context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "VACUUM;");`)
7. If the last `TODO 3` does not solve the slow saving in the main window (when deleting big collections) and the slow deletion of comics, add loading screens to other comics
8. Consider keeping windows visible, but "disabling them" like I do with the loading screen
