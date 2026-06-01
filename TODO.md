# URGENT

Fix **ALL** Search Bars (MainWindow and FilterTags), I forgot to implement a filter apparently lol

# Next

1. Disable auto_vacuum in db
2. Update db scheme rebuilding in code
3. Vacuum only when closing application (`context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "VACUUM;");`)
4. If the last `TODO 3` does not solve the slow saving in the main window (when deleting big collections) and the slow deletion of comics, add loading screens to other comics
5. Consider keeping windows visible, but "disabling them" like I do with the loading screen

