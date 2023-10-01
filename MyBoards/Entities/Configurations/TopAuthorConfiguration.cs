using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBoards.Entities.ViewModels;

namespace MyBoards.Entities.Configurations;

public class TopAuthorConfiguration : IEntityTypeConfiguration<TopAuthor>
{
    public void Configure(EntityTypeBuilder<TopAuthor> builder)
    {
        builder.ToView("View_TopAuthors");
        builder.HasNoKey();
    }
}
