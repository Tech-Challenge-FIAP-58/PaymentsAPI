using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Payments.Migrations
{
    /// <inheritdoc />
    public partial class ValidacaoDePagamentoEMudancasEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. REMOVER DEPENDÊNCIAS (Chaves e Índices)
            // Primeiro removemos a FK que liga as duas tabelas
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Payments_PaymentId",
                table: "Transactions");

            // Removemos as Chaves Primárias que trancam as colunas de ID
            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            // Removemos índices existentes
            migrationBuilder.DropIndex(
                name: "IX_Transactions_PaymentId",
                table: "Transactions");

            // 2. REMOVER COLUNAS ANTIGAS (Tipo INT)
            // Importante: Isso apagará os dados existentes nestas colunas
            migrationBuilder.DropColumn(name: "PaymentId", table: "Transactions");
            migrationBuilder.DropColumn(name: "Id", table: "Transactions");
            migrationBuilder.DropColumn(name: "Id", table: "Payments");
            migrationBuilder.DropColumn(name: "OrderId", table: "Payments");

            // 3. ADICIONAR NOVAS COLUNAS (Tipo GUID/uniqueidentifier)
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false);

            // 4. ALTERAR TIPOS E ADICIONAR CAMPOS DE STATUS/AUDITORIA
            // Alterando Enums de int para string (nvarchar)
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Payments",
                type: "nvarchar(20)", // Definido tamanho conforme boa prática
                nullable: false,
                defaultValue: "Pending");

            // Adicionando colunas de auditoria
            migrationBuilder.AddColumn<DateTime>(name: "CreatedAt", table: "Payments", type: "datetime2", nullable: false);
            migrationBuilder.AddColumn<DateTime>(name: "UpdatedAt", table: "Payments", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "IsDeleted", table: "Payments", type: "bit", nullable: false, defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(name: "CreatedAt", table: "Transactions", type: "datetime2", nullable: false);
            migrationBuilder.AddColumn<DateTime>(name: "UpdatedAt", table: "Transactions", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "IsDeleted", table: "Transactions", type: "bit", nullable: false, defaultValue: false);

            // 5. RECONSTRUIR REGRAS (PKs, FKs e Índices)
            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            // Índice Único Filtrado (Regra: Apenas 1 aprovado por OrderId)
            migrationBuilder.CreateIndex(
                name: "IX_Payment_OrderId",
                table: "Payments",
                column: "OrderId",
                unique: true,
                filter: "[Status] = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PaymentId",
                table: "Transactions",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Payments_PaymentId",
                table: "Transactions",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payment_OrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentId",
                table: "Transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "Payments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "Payments",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Payments",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");
        }
    }
}
