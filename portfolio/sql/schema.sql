CREATE SCHEMA IF NOT EXISTS portfolio;

DROP TABLE IF EXISTS portfolio.project_image;
DROP TABLE IF EXISTS portfolio.project;
DROP TABLE IF EXISTS portfolio.course;

-- COURSE
CREATE TABLE portfolio.course (
    course_id    bigint GENERATED ALWAYS AS IDENTITY,
    code         varchar(20)  NOT NULL,
    title        varchar(120) NOT NULL,
    description  text,
    term         varchar(30)  NOT NULL,
    school_year  varchar(20)  NOT NULL,
    CONSTRAINT pk_course PRIMARY KEY (course_id),
    CONSTRAINT uq_course_code UNIQUE (code),
    CONSTRAINT ck_course_code_format
        CHECK (code ~ '^[A-Z]{2,6}[ -]?[0-9]{2,4}[A-Z]?$'),
    CONSTRAINT ck_course_term
        CHECK (term IN ('1st Semester', '2nd Semester', 'Summer')),
    CONSTRAINT ck_course_school_year
        CHECK (school_year ~ '^[0-9]{4}-[0-9]{4}$')
);

-- PROJECT (a course has many projects)
CREATE TABLE portfolio.project (
    project_id    bigint GENERATED ALWAYS AS IDENTITY,
    course_id     bigint       NOT NULL,
    title         varchar(120) NOT NULL,
    description   text,
    video_url     varchar(300),
    completed_on  date,
    CONSTRAINT pk_project PRIMARY KEY (project_id),
    CONSTRAINT fk_project_course FOREIGN KEY (course_id)
        REFERENCES portfolio.course (course_id) ON DELETE RESTRICT,
    CONSTRAINT uq_project_course_title UNIQUE (course_id, title),
    CONSTRAINT ck_project_title_not_blank CHECK (length(trim(title)) > 0),
    CONSTRAINT ck_project_video_url
        CHECK (video_url IS NULL
               OR video_url ~* '^https?://(www\.)?(youtube\.com|youtu\.be)/')
);

-- PROJECT_IMAGE (a project has many images)
CREATE TABLE portfolio.project_image (
    image_id    bigint GENERATED ALWAYS AS IDENTITY,
    project_id  bigint       NOT NULL,
    image_url   varchar(500) NOT NULL,
    caption     varchar(150),
    sort_order  integer      NOT NULL DEFAULT 0,
    CONSTRAINT pk_project_image PRIMARY KEY (image_id),
    CONSTRAINT fk_project_image_project FOREIGN KEY (project_id)
        REFERENCES portfolio.project (project_id) ON DELETE CASCADE,
    CONSTRAINT uq_project_image_url UNIQUE (project_id, image_url),
    CONSTRAINT ck_project_image_url
        CHECK (image_url ~* '^https?://.+\.(jpg|jpeg|png|gif|webp)(\?.*)?$'),
    CONSTRAINT ck_project_image_sort CHECK (sort_order >= 0)
);

CREATE INDEX ix_project_course ON portfolio.project (course_id);
CREATE INDEX ix_project_image_project ON portfolio.project_image (project_id);