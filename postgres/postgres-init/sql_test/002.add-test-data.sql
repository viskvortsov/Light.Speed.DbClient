create table attributes
(
    id   uuid not null primary key,
    name uuid not null
);

alter table attributes
    owner to backend;

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

alter table attributes_translations
    owner to backend;

create table companies
(
    id          uuid not null primary key,
    currency_id uuid not null
);

alter table companies
    owner to backend;

create table currencies
(
    id         uuid         not null primary key,
    name       varchar(255) not null,
    deleted    varchar      not null,
    deleted_at timestamptz,
    upload     boolean,
    rate1      decimal,
    rate2      float,
    rate3      int,
    rate4      numeric,
    rate5      float
);

alter table currencies
    owner to backend;

create table exchange_rates
(
    id         uuid not null primary key,
    row_number int  not null,
    owner_id   uuid not null
);

alter table exchange_rates
    owner to backend;

create table currency_codes
(
    id       uuid    not null primary key,
    owner_id uuid    not null,
    code     varchar not null
);

alter table currency_codes
    owner to backend;

create table product_types
(
    id   int  not null PRIMARY KEY,
    name uuid not null
);

alter table product_types
    owner to backend;

create table product_type_translations
(
    language_id uuid not null,
    source_id   int  not null
        constraint owner_product_type
            references product_types
            on delete restrict,
    content_id  uuid not null,
    content     text not null,
    constraint product_type_translation_id
        primary key (language_id, source_id, content_id)
);

alter table product_type_translations
    owner to backend;

create table products
(
    id           uuid not null primary key,
    name         uuid not null,
    product_type int  not null
);

alter table products
    owner to backend;

create table product_attributes
(
    id        uuid not null primary key,
    owner_id  uuid not null,
    attribute uuid not null,
    value     uuid not null
);

alter table product_attributes
    owner to backend;

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

alter table products_translations
    owner to backend;

create table self_references
(
    id      uuid         not null,
    code    int GENERATED ALWAYS AS IDENTITY,
    name    varchar(255) not null,
    self_id uuid         not null,
    constraint self_reference_id
        primary key (id)
);

alter table self_references
    owner to backend;

create table prices
(
    product   uuid not null,
    variant   uuid not null,
    listprice numeric,
    saleprice numeric,
    constraint prices_pk primary key (product, variant)
);

alter table prices
    owner to backend;

create table enum_examples
(
    id    int  not null primary key,
    name  uuid not null,
    type1 int  not null,
    type2 int  not null,
    self_ref uuid not null
);

alter table enum_examples
    owner to backend;

create table enum_example_translations
(
    language_id uuid not null,
    source_id   int  not null
        constraint owner_enum_examples
            references enum_examples
            on delete restrict,
    content_id  uuid not null,
    content     text not null,
    constraint enum_example_translations_id
        primary key (language_id, source_id, content_id)
);

alter table enum_example_translations
    owner to backend;