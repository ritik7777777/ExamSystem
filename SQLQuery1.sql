select * from Users
delete from Users where id=7

create table Exams(
 ExamId	int Primary key Identity,
 ExamName varchar(100),
 TotalMarks int,
 duration int,
 CreatedDate datetime
)
select * from Exams

select * from Users


delete from  Users where id=9

ALTER TABLE Users
ALTER COLUMN Password VARCHAR(256)

UPDATE Users
SET Password = '03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4'
WHERE id = 5


Create table Questions(
	QuestionId int primary key identity,
	ExamId int,
	QuestionText varchar(1000),
	OptionA varchar(1000),
	OptionB varchar(1000),
	OptionC varchar(1000),
	OptionD varchar(1000),
	CorrectOption varchar(1),
)

create table Results(
	ResultId INT PRIMARY KEY IDENTITY,
    ExamId INT,
    StudentEmail VARCHAR(100),
    TotalQuestions INT,
    CorrectAnswers INT,
    Marks INT,
    ResultDate DATETIME
)
select * from Results
SELECT * FROM Questions WHERE ExamId = 1