create table attributes
(
    id uuid not null,
    name uuid not null,
    constraint attributes_id
        primary key (id)
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