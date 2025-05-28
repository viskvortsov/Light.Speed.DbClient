create table self_references
(
    id uuid not null,
    name varchar(255) not null,
    self_id uuid not null,
    constraint self_reference_id
        primary key (id)
);