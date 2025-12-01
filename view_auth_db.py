import sqlite3
import os

db_path = 'AuthService.API/AuthService.db'

if not os.path.exists(db_path):
    print(f"Database file not found: {db_path}")
    exit(1)

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Get all tables
cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
tables = cursor.fetchall()

print("=" * 60)
print("Tables in AuthService.db:")
print("=" * 60)

for table_name in tables:
    table = table_name[0]
    print(f"\n?? Table: {table}")
    print("-" * 60)
    
    # Get column info
    cursor.execute(f"PRAGMA table_info({table})")
    columns = cursor.fetchall()
    
    print("Columns:")
    for col in columns:
        col_id, col_name, col_type, not_null, default_val, pk = col
        pk_marker = " ?? PRIMARY KEY" if pk else ""
        nullable = "NOT NULL" if not_null else "NULL"
        print(f"  - {col_name} ({col_type}) {nullable}{pk_marker}")
    
    # Get row count
    cursor.execute(f"SELECT COUNT(*) FROM {table}")
    count = cursor.fetchone()[0]
    print(f"\n?? Total rows: {count}")
    
    # Show sample data (first 5 rows)
    if count > 0:
        cursor.execute(f"SELECT * FROM {table} LIMIT 5")
        rows = cursor.fetchall()
        col_names = [description[0] for description in cursor.description]
        
        print(f"\n?? Sample data (first {min(5, count)} rows):")
        for i, row in enumerate(rows, 1):
            print(f"\n  Row {i}:")
            for col_name, value in zip(col_names, row):
                display_value = str(value)[:50] + "..." if value and len(str(value)) > 50 else value
                print(f"    {col_name}: {display_value}")

print("\n" + "=" * 60)

conn.close()
