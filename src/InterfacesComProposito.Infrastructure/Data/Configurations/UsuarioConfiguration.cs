using InterfacesComProposito.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterfacesComProposito.Infrastructure.Data.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Ativo)
            .IsRequired();

        builder.Property(u => u.CriadoEm)
            .IsRequired();

        builder.OwnsOne(u => u.Endereco, endereco =>
        {
            endereco.Property(e => e.Logradouro)
                .HasColumnName("Endereco_Logradouro")
                .HasMaxLength(200);

            endereco.Property(e => e.Numero)
                .HasColumnName("Endereco_Numero")
                .HasMaxLength(20);

            endereco.Property(e => e.Complemento)
                .HasColumnName("Endereco_Complemento")
                .HasMaxLength(100);

            endereco.Property(e => e.Bairro)
                .HasColumnName("Endereco_Bairro")
                .HasMaxLength(100);

            endereco.Property(e => e.Cidade)
                .HasColumnName("Endereco_Cidade")
                .HasMaxLength(100);

            endereco.Property(e => e.Estado)
                .HasColumnName("Endereco_Estado")
                .HasMaxLength(2);

            endereco.Property(e => e.Cep)
                .HasColumnName("Endereco_Cep")
                .HasMaxLength(10);
        });
    }
}
