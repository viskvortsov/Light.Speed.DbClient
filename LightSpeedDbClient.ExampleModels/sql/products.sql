create table products
(
    id uuid not null,
    name uuid not null,
    product_type int not null,
    constraint products_id
        primary key (id)
);

create table product_attributes
(
    id uuid not null,
    owner_id uuid not null,
    attribute uuid not null,
    value uuid not null,
    constraint product_attributes_id
        primary key (id)
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