using InterfacesComProposito.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterfacesComProposito.Infrastructure.Data.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Preco)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Ativo)
            .IsRequired();
    }
}
