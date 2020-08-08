# seeder
Generate SQL seed script for your enumeration tables.

## Security Warning
If your seed data source (aka enumeration tables) contains any unescaped SQL strings, your generated SQL script may contain SQL injection. It's your responsibility to validate and escape your enumeration table data for possible SQL injection attacks.
