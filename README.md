# How to run

```docker compose up``` inside root directory (current)

Swagger is available [here](http://localhost:8080/swagger)


if it won't work out of the box, its possible that there is a need to create a migration for the db (unlikely)

## Important notice

some of the configurations are not production ready and are done for simpliciy of testing the solution
(e.g.
- auto migration
- storing keys in docker compose
- code hardcoded admin credentials
- no SSL, simple cors
- etc.
