INSERT INTO portfolio.course (code, title, description, term, school_year) VALUES
('IT101', 'Introduction to Computing', 'Basics of computers, programming logic and the web.', '1st Semester', '2024-2025'),
('IT201', 'Data Structures and Algorithms', 'Arrays, lists, trees, sorting and searching.', '2nd Semester', '2024-2025'),
('IT301', 'Database Management and Web Systems', 'Relational design, SQL and ASP.NET Core MVC.', '1st Semester', '2026-2027');

INSERT INTO portfolio.project (course_id, title, description, video_url, completed_on)
SELECT course_id, 'Personal Website', 'A simple HTML and CSS personal page.',
       'https://www.youtube.com/watch?v=dQw4w9WgXcQ', DATE '2024-10-30'
FROM portfolio.course WHERE code = 'IT101';

INSERT INTO portfolio.project (course_id, title, description, video_url, completed_on)
SELECT course_id, 'Sorting Visualizer', 'Animates bubble, merge and quick sort.',
       NULL, DATE '2025-04-20'
FROM portfolio.course WHERE code = 'IT201';

INSERT INTO portfolio.project (course_id, title, description, video_url, completed_on)
SELECT course_id, 'Library System', 'A library app with borrowing and returns.',
       'https://youtu.be/dQw4w9WgXcQ', DATE '2026-09-15'
FROM portfolio.course WHERE code = 'IT301';

INSERT INTO portfolio.project_image (project_id, image_url, caption, sort_order)
SELECT project_id, 'https://picsum.photos/id/1/800/450.jpg', 'Home page', 1
FROM portfolio.project WHERE title = 'Personal Website';

INSERT INTO portfolio.project_image (project_id, image_url, caption, sort_order)
SELECT project_id, 'https://picsum.photos/id/20/800/450.jpg', 'About page', 2
FROM portfolio.project WHERE title = 'Personal Website';

INSERT INTO portfolio.project_image (project_id, image_url, caption, sort_order)
SELECT project_id, 'https://picsum.photos/id/180/800/450.jpg', 'Sorting in action', 1
FROM portfolio.project WHERE title = 'Sorting Visualizer';

INSERT INTO portfolio.project_image (project_id, image_url, caption, sort_order)
SELECT project_id, 'https://picsum.photos/id/119/800/450.jpg', 'Book list', 1
FROM portfolio.project WHERE title = 'Library System';

INSERT INTO portfolio.project_image (project_id, image_url, caption, sort_order)
SELECT project_id, 'https://picsum.photos/id/160/800/450.jpg', 'Borrow screen', 2
FROM portfolio.project WHERE title = 'Library System';