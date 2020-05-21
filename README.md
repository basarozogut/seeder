# seeder
Generate SQL seed script for your enumeration tables.

## Security Warning
Your seed data source (aka enumeration tables) must not include any SQL strings. It's your responsibility to check and validate your enumeration table data for possible SQL injection attacks. There are some precautions for preventing possible injection attacks but the dynamic nature of the code generator will just append the enumeration strings and possible SQL injection attacks may be included in the generated script.
