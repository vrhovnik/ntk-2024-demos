create table Categories
(
    CategoryId uniqueidentifier not null rowguidcol,
    Name       nvarchar(max)    not null
)
    go

alter table Categories
    add constraint Categories_pk
        primary key (CategoryId)
    go

create table LinkGroups
(
    LinkGroupId uniqueidentifier not null rowguidcol,
    Name        nvarchar(max)    not null,
    Description nvarchar(max),
    ShortName   nvarchar(50)     not null,
    UserId      uniqueidentifier,
    Clicked     int default 0    not null,
    CategoryId  uniqueidentifier not null,
    CreatedAt   datetime         not null
)
    go

alter table LinkGroups
    add constraint LinkGroups_pk
        primary key (LinkGroupId)
    go

alter table LinkGroups
    add constraint LinkGroups_Categories_CategoryId_fk
        foreign key (CategoryId) references Categories
    go

create table Links
(
    LinkId      uniqueidentifier not null rowguidcol,
    Name        nvarchar(max)    not null,
    Url         nvarchar(max)    not null,
    LinkGroupId uniqueidentifier not null
)
    go

alter table Links
    add constraint Links_pk
        primary key (LinkId)
    go

alter table Links
    add constraint Links_LinkGroups_LinkGroupId_fk
        foreign key (LinkGroupId) references LinkGroups
    go

create table Users
(
    UserId   uniqueidentifier not null rowguidcol,
    FullName nvarchar(max),
    Email    nvarchar(max)    not null,
    Password nvarchar(max)    not null
)
    go

alter table Users
    add constraint Users_pk
        primary key (UserId)
    go

alter table LinkGroups
    add constraint LinkGroups_Users_UserId_fk
        foreign key (UserId) references Users
    go