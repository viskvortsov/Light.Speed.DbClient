#!/bin/sh

##########################################################
##       Unarchiving adn restoring to container DB      ##
##########################################################

set -x

############## Base #############

echo "Initializing local PostgreSQL database - restoring main sql scripts..."
for file in `find /sql/sql_base/ | grep -i '\.sql' | sort`
do 
  echo "Restoring $file..."
  psql < $file
done

############## Prod #############

if [[ "${FILL_PROD_DATA}" == "true" ]]; then

    for file in `find /sql/sql_prod/ | grep -i '\.sql' | sort`
    do
      echo "Copy prod data from $file to local DB..."
      psql -d $DB_NAME -U $DB_USER_NAME < $file
    done
fi

############ Non-prod ############

if [[ "${FILL_TEST_DATA}" == "true" ]]; then

    for file in `find /sql/sql_test/ | grep -i '\.sql' | sort`
    do
      echo "Copy non-prod data from $file to local DB..."
      psql -d $DB_NAME -U $DB_USER_NAME < $file
    done
fi

############ Triggers ############

echo "Initializing local PostgreSQL - enabling triggers..."
for file in `find /sql/sql_finalizers/ | grep -i '\.sql' | sort`
do
  echo "Restoring $file..."
  psql < $file
done

############ ALTER backend user password ############

psql -d $DB_NAME -U $DB_USER_NAME -c "ALTER USER backend WITH PASSWORD 'mysecretpassword'"
