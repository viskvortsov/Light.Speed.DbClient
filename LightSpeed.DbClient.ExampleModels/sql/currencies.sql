create table currencies
(
    id uuid not null,
    name uuid not null,
    deleted varchar not null,
    deleted_at timestamptz,
    upload boolean,
    rate1 decimal,
    rate2 float,
    rate3 int,
    rate4 numeric,
    rate5 float,
    constraint attributes_id
        primary key (id)
);

create table exchange_rates
(
    id uuid not null,
    row_number int not null,
    owner_id  uuid not null
);

create table currency_codes
(
    id uuid not null,
    row_number int not null,
    owner_id  uuid not null,
    code varchar not null
);