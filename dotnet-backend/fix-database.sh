#!/bin/bash
# Script para corregir el esquema de la base de datos

echo "Aplicando migraciones a la base de datos..."

# Aplicar migraciones
dotnet ef database update -p src/Infrastructure -s src/Api

echo "Migraciones aplicadas correctamente."
