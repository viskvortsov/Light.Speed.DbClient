#!/bin/sh

docker compose -f dev-env.yml down backend pgbouncer
if [ -d build/dev/data/postgresql ]; then rm -Rf build/dev/data/postgresql; fi
if [ -d build/dev/data/adminer ]; then rm -Rf build/dev/data/adminer; fi
mkdir build/dev
mkdir build/dev/data
chown -R 777 build/dev 
chmod -R 777 build/dev
docker compose rm -fv db 
docker compose --compatibility -f dev-env.yml up -d backend pgbouncer --build
