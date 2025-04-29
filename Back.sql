--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: CorrectAnswers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."CorrectAnswers" (
    "CorrectAnswerId" integer NOT NULL,
    "Answer" character varying(500)
);


ALTER TABLE public."CorrectAnswers" OWNER TO postgres;

--
-- Name: CorrectAnswers_CorrectAnswerId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."CorrectAnswers" ALTER COLUMN "CorrectAnswerId" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."CorrectAnswers_CorrectAnswerId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Education; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Education" (
    "EducationId" integer NOT NULL,
    "EducationLink" character varying(500),
    "Theme" character varying(500)
);


ALTER TABLE public."Education" OWNER TO postgres;

--
-- Name: Education_EducationId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Education" ALTER COLUMN "EducationId" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Education_EducationId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Questions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Questions" (
    "QuestionId" integer NOT NULL,
    "TestTitleId" integer NOT NULL,
    "CorrectAnswerId" integer NOT NULL,
    "Issue" character varying(500) DEFAULT ''::character varying,
    "IssueChoice1" character varying(500),
    "IssueChoice2" character varying(500),
    "IssueChoice3" character varying(500),
    "IssueChoice4" character varying(500)
);


ALTER TABLE public."Questions" OWNER TO postgres;

--
-- Name: Questions_QuestionId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Questions" ALTER COLUMN "QuestionId" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Questions_QuestionId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: TestTitles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TestTitles" (
    "TestTitleId" integer NOT NULL,
    "Title" character varying(500) NOT NULL
);


ALTER TABLE public."TestTitles" OWNER TO postgres;

--
-- Name: TestTitles_TestTitleId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."TestTitles" ALTER COLUMN "TestTitleId" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."TestTitles_TestTitleId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: UserProgresses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."UserProgresses" (
    "UserProgressId" integer NOT NULL,
    "UserId" integer NOT NULL,
    "TestTitleId" integer NOT NULL,
    "IsCompleted" boolean NOT NULL
);


ALTER TABLE public."UserProgresses" OWNER TO postgres;

--
-- Name: UserProgresses_UserProgressId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."UserProgresses" ALTER COLUMN "UserProgressId" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."UserProgresses_UserProgressId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Users" (
    "UserId" integer NOT NULL,
    "FullName" character varying(200),
    "PhoneNumber" character varying(50),
    "ChatId" integer DEFAULT 0 NOT NULL
);


ALTER TABLE public."Users" OWNER TO postgres;

--
-- Name: Users_UserId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Users" ALTER COLUMN "UserId" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Users_UserId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Data for Name: CorrectAnswers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."CorrectAnswers" ("CorrectAnswerId", "Answer") FROM stdin;
4	d
3	c
2	b
1	a
\.


--
-- Data for Name: Education; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Education" ("EducationId", "EducationLink", "Theme") FROM stdin;
1	https://telegra.ph/Blok-3-Upravlenie-zadachami-04-26	Управление Задачами
2	https://telegra.ph/Blok-1-navyk-tajm-menedzhment-04-26	Навык Тайм-менеджмент
3	https://telegra.ph/Blok-2-Navyk-Liderstvo-04-26	Навык Лидерство
4	https://telegra.ph/Matrica-EHjzenhauehra-04-26	Навык Тайм-менеджмент
5	https://telegra.ph/Metod-ABCDE-04-26	Навык Тайм-менеджмент
6	https://telegra.ph/Delegirovanie-04-26	Управление Задачами
7	https://telegra.ph/Stili-liderstva-04-26	Навык Лидерство
8	https://telegra.ph/Delegirovanie-04-26	Управление Задачами
\.


--
-- Data for Name: Questions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Questions" ("QuestionId", "TestTitleId", "CorrectAnswerId", "Issue", "IssueChoice1", "IssueChoice2", "IssueChoice3", "IssueChoice4") FROM stdin;
2	1	2	Какой из следующих инструментов лучше всего подходит для визуализации задач и их сроков?	Календарь	Доска задач (Kanban)	Тетрадь	\N
1	1	1	Какой из следующих подходов наиболее эффективен для определения приоритетов задач?\n	Метод ABCDE	Метод "Сделай это позже"	Метод "Случайного выбора"	\N
3	1	1	Какой из следующих методов помогает в управлении проектами и задачами в команде?	Agile	Метод "Слепого выполнения"	Метод "Ожидания вдохновения"	\N
4	1	1	Какой из следующих подходов может помочь в улучшении личной продуктивности?	Регулярные рефлексии и анализ выполненных задач	 Постоянное выполнение задач в порядке их поступления	Игнорирование сроков выполнения задач	null
13	1	1	Какой из следующих методов позволяет оценить, насколько эффективно вы используете свое время?	Ведение журнала времени	Составление списка дел	Оценка по интуиции	
22	8	2	Какой стиль лидерства характеризуется высокой степенью контроля со стороны лидера и низким уровнем вовлеченности команды?	Демократический	Авторитарный	Либеральный	Трансформационный
23	8	4	Какой стиль лидерства сочетает в себе элементы различных стилей в зависимости от ситуации?	Трансакционный	Ситуационный	Демократический	Либеральный
24	8	4	Какой стиль лидерства может быть наиболее эффективным в условиях неопределенности и быстроменяющейся среды?	Трансформационный	Авторитарный	Либеральный	Ситуационный
25	8	2	Какой стиль лидерства может быть наиболее эффективным в творческих командах, где важна свобода действий?	Авторитарный	Либеральный	Трансакционный	Патерналистский
26	8	2	Какой стиль лидерства может привести к высокой зависимости сотрудников от мнения лидера?	Либеральный	Патерналистский	Демократический	Трансформационный
27	9	2	Какой из следующих подходов лучше всего описывает SMART-цели?	Цели должны быть сложными и труднодостижимыми	Цели должны быть конкретными, измеримыми, достижимыми, актуальными и ограниченными во времени	Цели должны быть общими и неформальными	Цели должны быть определены только на уровне команды
28	9	4	Какой из следующих факторов не является важным при делегировании задач?	Уровень компетенции сотрудника	Наличие времени у руководителя	Четкость инструкции и ожиданий	Личное отношение к сотруднику
29	9	2	Какой из следующих методов лучше всего подходит для оценки прогресса выполнения задачи?	Ожидать, пока задача будет завершена	Проводить регулярные проверки и обсуждения статуса	Не обращать внимания на прогресс, пока не наступит срок	Делегировать задачу и забыть о ней
30	9	2	Какой из следующих подходов является наилучшим для делегирования задач?	Делегировать только рутинные задачи	Делегировать задачи, которые могут помочь сотруднику развить новые навыки	Делегировать задачи, которые вам не нравятся	Делегировать задачи без объяснения их важности
31	9	1	Какой из следующих вариантов является примером эффективной постановки задачи?	"Пожалуйста, подготовь отчет о продажах за последний квартал к следующей пятнице"	"Сделай что-то с отчетом о продажах"	"Нужно улучшить продажи"	"Подготовь презентацию для встречи с клиентами на следующей неделе"
32	9	1	Какой из следующих методов может помочь в делегировании задач?	Обсуждение с сотрудником его сильных и слабых сторон	Делегирование всех задач без объяснений	Установка жестких сроков без учета мнения сотрудника	Избегание делегирования, чтобы сохранить контроль
33	9	2	Какой из следующих подходов лучше всего подходит для обеспечения ясности в задаче?	Использовать общие формулировки	Предоставить четкие инструкции и ожидания	Оставить все на усмотрение сотрудника	Не обсуждать задачу до ее завершения
34	9	2	Какой из следующих факторов может способствовать успешному делегированию?	Непонимание задач	Поддержка и обратная связь от руководителя	Отсутствие контроля за выполнением задач	Игнорирование вопросов сотрудников
35	9	2	Какой из следующих вариантов является примером эффективного делегирования?	"Я не могу это сделать, поэтому сделай это ты"	"Я доверяю тебе, и ты можешь взять на себя эту задачу, если у тебя есть время"	"Я не хочу этим заниматься, поэтому делегирую это тебе"	"Ты должен сделать это, потому что я так сказал"
36	9	2	Какой из следующих методов может помочь в повышении мотивации сотрудников при выполнении делегированных задач?	Игнорировать их усилия	Предоставить возможность для обучения и развития	Установить жесткие сроки без обсуждения	Не давать обратной связи
\.


--
-- Data for Name: TestTitles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."TestTitles" ("TestTitleId", "Title") FROM stdin;
1	Навык Тайм-менеджмент
8	Навык Лидерство
9	Управление Задачами
\.


--
-- Data for Name: UserProgresses; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."UserProgresses" ("UserProgressId", "UserId", "TestTitleId", "IsCompleted") FROM stdin;
33	7	1	f
34	7	8	f
35	7	9	f
40	9	8	f
41	9	9	f
39	9	1	t
42	10	1	f
43	10	8	f
44	10	9	f
45	11	1	f
46	11	8	f
47	11	9	f
\.


--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Users" ("UserId", "FullName", "PhoneNumber", "ChatId") FROM stdin;
7	Егор У	+79831582961	467628435
9	Мингатина Самира Рустамовна	79196800683	908634620
10	Еремина Дарья Сергеевна	79172832257	397768028
11	Чернов Илья Сергеевич	+79623214239	946851965
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20250421160006_InitialCreate	9.0.4
20250421160206_SecondMigration	9.0.4
20250421160336_SecondMigration	9.0.4
20250425153242_AutoIncrement	9.0.4
20250427175954_AddFieldInUser	9.0.4
20250427114105_newInit	9.0.4
20250427203646_last	9.0.4
20250428102018_AddEducationEntity	9.0.4
20250428102531_AddFieldInEducationEntity	9.0.4
20250428115924_AddLongread	9.0.4
\.


--
-- Name: CorrectAnswers_CorrectAnswerId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."CorrectAnswers_CorrectAnswerId_seq"', 1, false);


--
-- Name: Education_EducationId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Education_EducationId_seq"', 8, true);


--
-- Name: Questions_QuestionId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Questions_QuestionId_seq"', 36, true);


--
-- Name: TestTitles_TestTitleId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."TestTitles_TestTitleId_seq"', 9, true);


--
-- Name: UserProgresses_UserProgressId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."UserProgresses_UserProgressId_seq"', 47, true);


--
-- Name: Users_UserId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Users_UserId_seq"', 11, true);


--
-- Name: CorrectAnswers PK_CorrectAnswers; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."CorrectAnswers"
    ADD CONSTRAINT "PK_CorrectAnswers" PRIMARY KEY ("CorrectAnswerId");


--
-- Name: Education PK_Education; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Education"
    ADD CONSTRAINT "PK_Education" PRIMARY KEY ("EducationId");


--
-- Name: Questions PK_Questions; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Questions"
    ADD CONSTRAINT "PK_Questions" PRIMARY KEY ("QuestionId");


--
-- Name: TestTitles PK_TestTitles; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestTitles"
    ADD CONSTRAINT "PK_TestTitles" PRIMARY KEY ("TestTitleId");


--
-- Name: UserProgresses PK_UserProgresses; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserProgresses"
    ADD CONSTRAINT "PK_UserProgresses" PRIMARY KEY ("UserProgressId");


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("UserId");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_Questions_CorrectAnswerId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Questions_CorrectAnswerId" ON public."Questions" USING btree ("CorrectAnswerId");


--
-- Name: IX_Questions_TestTitleId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Questions_TestTitleId" ON public."Questions" USING btree ("TestTitleId");


--
-- Name: IX_UserProgresses_TestTitleId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_UserProgresses_TestTitleId" ON public."UserProgresses" USING btree ("TestTitleId");


--
-- Name: IX_UserProgresses_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_UserProgresses_UserId" ON public."UserProgresses" USING btree ("UserId");


--
-- Name: Questions FK_Questions_CorrectAnswers_CorrectAnswerId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Questions"
    ADD CONSTRAINT "FK_Questions_CorrectAnswers_CorrectAnswerId" FOREIGN KEY ("CorrectAnswerId") REFERENCES public."CorrectAnswers"("CorrectAnswerId") ON DELETE CASCADE;


--
-- Name: Questions FK_Questions_TestTitles_TestTitleId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Questions"
    ADD CONSTRAINT "FK_Questions_TestTitles_TestTitleId" FOREIGN KEY ("TestTitleId") REFERENCES public."TestTitles"("TestTitleId") ON DELETE CASCADE;


--
-- Name: UserProgresses FK_UserProgresses_TestTitles_TestTitleId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserProgresses"
    ADD CONSTRAINT "FK_UserProgresses_TestTitles_TestTitleId" FOREIGN KEY ("TestTitleId") REFERENCES public."TestTitles"("TestTitleId") ON DELETE CASCADE;


--
-- Name: UserProgresses FK_UserProgresses_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserProgresses"
    ADD CONSTRAINT "FK_UserProgresses_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("UserId") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

