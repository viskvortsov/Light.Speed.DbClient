create table attributes
(
    id uuid not null primary key,
    name uuid not null
);

create table attributes_translations
(
    language_id uuid not null,
    source_id   uuid not null
        constraint owner_attributes
            references attributes
            on delete restrict,
    content_id  uuid not null,
    content     text not null,
    constraint attributes_translation_id
        primary key (language_id, source_id, content_id)
);

create table companies
(
    id uuid not null primary key,
    currency_id uuid not null
);

create table currencies
(
    id uuid not null primary key,
    name varchar(255) not null,
    deleted varchar not null,
    deleted_at timestamptz,
    upload boolean,
    rate1 decimal,
    rate2 float,
    rate3 int,
    rate4 numeric,
    rate5 float
);

create table exchange_rates
(
    id uuid not null primary key,
    row_number int not null,
    owner_id  uuid not null
);

create table currency_codes
(
    id uuid not null primary key,
    owner_id  uuid not null,
    code varchar not null
);

create table product_types
(
    id int not null PRIMARY KEY,
    name   uuid not null
);

create table product_type_translations
(
    language_id uuid not null,
    source_id   int not null
        constraint owner_product_type
            references product_types
            on delete restrict,
    content_id  uuid not null,
    content     text not null,
    constraint product_type_translation_id
        primary key (language_id, source_id, content_id)
);

create table products
(
    id uuid not null primary key,
    name uuid not null,
    product_type int not null
);

create table product_attributes
(
    id uuid not null primary key,
    owner_id uuid not null,
    attribute uuid not null,
    value uuid not null
);

create table products_translations
(
    language_id uuid not null,
    source_id   uuid not null
        constraint owner_products
            references products
            on delete restrict,
    content_id  uuid not null,
    content     text not null,
    constraint products_translation_id
        primary key (language_id, source_id, content_id)
);

create table self_references
(
    id uuid not null,
    name varchar(255) not null,
    self_id uuid not null,
    constraint self_reference_id
        primary key (id)
);