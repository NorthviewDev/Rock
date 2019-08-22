using Rock.Plugin;

namespace us.northviewchurch.Migrations
{
    [MigrationNumber(1, "1.6.0")]
    public class AddPrayerBatchTable : Migration
    {
        public override void Up()
        {
            //Add the PrayerBatch Table
            Sql(@"
    CREATE TABLE [dbo].[_us_northviewchurch_PrayerBatch](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [Active] [bit] NOT NULL,	 
        [CompletionDate] [datetime] NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [ForeignKey] [nvarchar](50) NULL,
	    [ForeignGuid] [uniqueidentifier] NULL,
        [ForeignId] [nvarchar](50) NULL,
     CONSTRAINT [PK_dbo._us_northviewchurch_PrayerBatch] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
    )

    ALTER TABLE [dbo].[_us_northviewchurch_PrayerBatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo._us_northviewchurch_PrayerBatch_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_us_northviewchurch_PrayerBatch] CHECK CONSTRAINT [FK_dbo._us_northviewchurch_PrayerBatch_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_us_northviewchurch_PrayerBatch]  WITH CHECK ADD  CONSTRAINT [FK_dbo._us_northviewchurch_PrayerBatch_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_us_northviewchurch_PrayerBatch] CHECK CONSTRAINT [FK_dbo._us_northviewchurch_PrayerBatch_dbo.PersonAlias_ModifiedByPersonAliasId]
");

            
        }

        public override void Down()
        {           
            //Remove the PrayerBatchTable
            Sql(@"
    ALTER TABLE [dbo].[_us_northviewchurch_PrayerBatch] DROP CONSTRAINT [FK_dbo._us_northviewchurch_PrayerBatch_dbo.PersonAlias_ModifiedByPersonAliasId]
    ALTER TABLE [dbo].[_us_northviewchurch_PrayerBatch] DROP CONSTRAINT [FK_dbo._us_northviewchurch_PrayerBatch_dbo.PersonAlias_CreatedByPersonAliasId]    
    DROP TABLE [dbo].[_us_northviewchurch_PrayerBatch]
");
        }
    }
}
