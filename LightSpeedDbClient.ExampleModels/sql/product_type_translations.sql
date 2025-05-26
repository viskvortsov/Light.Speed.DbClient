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