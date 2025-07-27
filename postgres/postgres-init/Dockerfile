FROM postgres:15.4-alpine3.18

COPY --chown=postgres ./data /data
COPY --chown=postgres ./sql_base /sql/sql_base
COPY --chown=postgres ./sql_test /sql/sql_test
COPY --chown=postgres ./sql_prod /sql/sql_prod
COPY --chown=postgres ./sql_finalizers /sql/sql_finalizers
