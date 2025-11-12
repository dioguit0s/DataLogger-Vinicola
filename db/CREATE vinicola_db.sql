create database vinicola_db;

create table user
	(
	id int not null primary key,
	nome VARCHAR(50) not null,
	password_hash VARCHAR(max) not null,

	)

create table dataLogger
	(
	id int not null primary key,
	user_id int not null,
	temp_min decimal(10,2) not null,
	temp_max decimal(10,2) not null,
	lum_min decimal(10,2) not null,
	lum_max decimal(10,2) not null,
	humid_min decimal(10,2) not null,
	humid_max decimal(10,2) not null
	)

create table errorLog
	(
	id int not null,
	datalogger_id int not null,
	time timestamp not null,
	temp decimal(10,2) not null,
	lum decimal(10,2) not null,
	humid decimal(10,2) not null,
	error_temp bit default 0 not null,
	error_humid bit default 0 not null,
	error_lum bit default 0 not null
	)