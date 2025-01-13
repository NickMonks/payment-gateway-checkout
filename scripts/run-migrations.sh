#!/bin/bash

set -e

echo "PostgreSQL is ready. Running migrations..."

# Run Entity Framework migrations
dotnet ef database update

echo "Migrations completed."