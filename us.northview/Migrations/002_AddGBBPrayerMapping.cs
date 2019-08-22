using Rock.Plugin;

namespace us.northviewchurch.Migrations
{
    [MigrationNumber(2, "1.6.0")]
    public class AddGBBPrayerMapping : Migration
    {
        public override void Down()
        {
            //Remove the GBBPrayerRequestMapping
            Sql(@"
    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping] DROP CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_dbo.PersonAlias_ModifiedByPersonAliasId]
    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping] DROP CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_dbo.PersonAlias_CreatedByPersonAliasId]    
    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping] DROP CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_us_northviewchurch_PrayerBatch_Id]    
    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping] DROP CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_dbo.PrayerRequest_Id]    
    DROP TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping]
");
        }

        public override void Up()
        {
            //Add the GBBPrayerRequestMapping Table
            Sql(@"
    CREATE TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [PrayerBatchId] [int] NOT NULL,	    
        [RockPrayerRequestId] [int] NOT NULL,
        [PrayerPartnerId] [int] NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [ForeignKey] [nvarchar](50) NULL,
	    [ForeignGuid] [uniqueidentifier] NULL,
        [ForeignId] [nvarchar](50) NULL,
     CONSTRAINT [PK_dbo._us_northviewchurch_GBBPrayerRequestMapping] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
    )

    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping]  WITH CHECK ADD  CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping] CHECK CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping]  WITH CHECK ADD  CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping] CHECK CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_dbo.PersonAlias_ModifiedByPersonAliasId]

    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping]  WITH CHECK ADD  CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_us_northviewchurch_PrayerBatch_Id] FOREIGN KEY([PrayerBatchId])
    REFERENCES [dbo].[_us_northviewchurch_PrayerBatch] ([Id])

    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping] CHECK CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_us_northviewchurch_PrayerBatch_Id]

    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping]  WITH CHECK ADD  CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_dbo.PrayerRequest_Id] FOREIGN KEY([RockPrayerRequestId])
    REFERENCES [dbo].[PrayerRequest] ([Id])

    ALTER TABLE [dbo].[_us_northviewchurch_GBBPrayerRequestMapping] CHECK CONSTRAINT [FK_dbo._us_northviewchurch_GBBPrayerRequestMapping_dbo.PrayerRequest_Id]
");

        }
    }
}
