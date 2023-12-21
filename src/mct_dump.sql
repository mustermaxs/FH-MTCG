--
-- PostgreSQL database dump
--

-- Dumped from database version 14.10 (Ubuntu 14.10-0ubuntu0.22.04.1)
-- Dumped by pg_dump version 14.10 (Ubuntu 14.10-0ubuntu0.22.04.1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA public;


--
-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: actions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.actions (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    actiontype character varying(250)
);


ALTER TABLE public.actions OWNER TO postgres;

--
-- Name: authtokes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.authtokes (
    id integer NOT NULL,
    token character varying(50) NOT NULL,
    userid uuid
);


ALTER TABLE public.authtokes OWNER TO postgres;

--
-- Name: authtokes_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.authtokes_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.authtokes_id_seq OWNER TO postgres;

--
-- Name: authtokes_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.authtokes_id_seq OWNED BY public.authtokes.id;


--
-- Name: battles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.battles (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    userid uuid,
    opponentid uuid,
    winner uuid,
    isdraw boolean,
    enddatetime timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    countrounds smallint
);


ALTER TABLE public.battles OWNER TO postgres;

--
-- Name: cards; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cards (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    descr text,
    damage numeric,
    type character(30),
    element character varying(50),
    name character varying(30)
);


ALTER TABLE public.cards OWNER TO postgres;

--
-- Name: deck; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.deck (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    cardid uuid,
    userid uuid,
    countcardsoftype smallint
);


ALTER TABLE public.deck OWNER TO postgres;

--
-- Name: elementtypes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.elementtypes (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    elementname character(50)
);


ALTER TABLE public.elementtypes OWNER TO postgres;

--
-- Name: roles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.roles (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    role character varying(15)
);


ALTER TABLE public.roles OWNER TO postgres;

--
-- Name: usercards; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.usercards (
    userid uuid NOT NULL,
    cardid uuid
);


ALTER TABLE public.usercards OWNER TO postgres;

--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    name character varying(30) NOT NULL,
    password character varying(65) NOT NULL,
    coins smallint,
    bio character varying(150),
    image character varying(30),
    role uuid
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: authtokes id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.authtokes ALTER COLUMN id SET DEFAULT nextval('public.authtokes_id_seq'::regclass);


--
-- Data for Name: actions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.actions (id, actiontype) FROM stdin;
\.


--
-- Data for Name: authtokes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.authtokes (id, token, userid) FROM stdin;
\.


--
-- Data for Name: battles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.battles (id, userid, opponentid, winner, isdraw, enddatetime, countrounds) FROM stdin;
\.


--
-- Data for Name: cards; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cards (id, descr, damage, type, element, name) FROM stdin;
f1f0a154-0d70-41cc-a177-d9ee24c190c0		10	monster                       	water	WaterGoblin
f9256ae7-63a9-4c4c-95a8-538fcda76b39		15	monster                       	fire	FireGoblin
c3bfa8ad-daf1-4e71-995c-cd2983b1e0d9		10	monster                       	normal	Knight
2342238b-360e-49db-b9c5-2abe42ca0f7d		20	monster                       		Dragon
\.


--
-- Data for Name: deck; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.deck (id, cardid, userid, countcardsoftype) FROM stdin;
\.


--
-- Data for Name: elementtypes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.elementtypes (id, elementname) FROM stdin;
ca2cde7d-4837-4783-abcf-ae3fe2230d40	fire                                              
d0e292b1-130d-490a-ab70-f301aee6158c	water                                             
e607cad1-6719-401e-9ecd-d96fc62d6a6e	normal                                            
33f48c45-ddd5-4a0a-a630-0442ce245e34	monster                                           
9a7e50ec-67d1-4be1-a6d0-322af892b5b6	spell                                             
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.roles (id, role) FROM stdin;
\.


--
-- Data for Name: usercards; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.usercards (userid, cardid) FROM stdin;
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, name, password, coins, bio, image, role) FROM stdin;
897cb65a-4381-4c14-afda-65ff2cd291a4	Michael	mikey	32	Halloooo.	###	\N
ab7f603c-2555-4d44-8469-257687eceac6	Max	mustermax	100	Top.	###	\N
5393eac1-4180-43fb-bf58-8b8dc5bcec88	Markus	Roesi	100	Miau	###	\N
c8b88625-c94c-4551-86e8-f371e1244335	max	maxiking	100	Miau	###	\N
74aebcbb-bebd-42dc-912b-2cc2b5a0e697	maxi	aafa757776d8c167d9b360dc8362b3468049701bb47db65229b2055fae7b5146	100	Miau	###	\N
d324b594-06d6-42e2-b921-f371180edb8b	winti	f6bd083b0a55b6523a1348233369c5392d925f0d5cf5cbe7d270faa8600a865f	100	Miau	###	\N
\.


--
-- Name: authtokes_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.authtokes_id_seq', 1, false);


--
-- Name: actions actions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.actions
    ADD CONSTRAINT actions_pkey PRIMARY KEY (id);


--
-- Name: authtokes authtokes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.authtokes
    ADD CONSTRAINT authtokes_pkey PRIMARY KEY (id);


--
-- Name: battles battles_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT battles_pkey PRIMARY KEY (id);


--
-- Name: cards cards_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cards
    ADD CONSTRAINT cards_pkey PRIMARY KEY (id);


--
-- Name: deck deck_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deck
    ADD CONSTRAINT deck_pkey PRIMARY KEY (id);


--
-- Name: elementtypes elementtypes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.elementtypes
    ADD CONSTRAINT elementtypes_pkey PRIMARY KEY (id);


--
-- Name: roles roles_primary_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_primary_key PRIMARY KEY (id);


--
-- Name: authtokes unique_userid; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.authtokes
    ADD CONSTRAINT unique_userid UNIQUE (userid);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (name);


--
-- Name: authtokes fk_authtoken_userid; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.authtokes
    ADD CONSTRAINT fk_authtoken_userid FOREIGN KEY (userid) REFERENCES public.users(id);


--
-- Name: deck fk_cardid_deck; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deck
    ADD CONSTRAINT fk_cardid_deck FOREIGN KEY (cardid) REFERENCES public.cards(id);


--
-- Name: usercards fk_cardid_usercards; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.usercards
    ADD CONSTRAINT fk_cardid_usercards FOREIGN KEY (cardid) REFERENCES public.cards(id);


--
-- Name: battles fk_opponentid_battle; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT fk_opponentid_battle FOREIGN KEY (opponentid) REFERENCES public.users(id);


--
-- Name: users fk_roles; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT fk_roles FOREIGN KEY (role) REFERENCES public.roles(id);


--
-- Name: battles fk_userid_battle; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT fk_userid_battle FOREIGN KEY (userid) REFERENCES public.users(id);


--
-- Name: deck fk_userid_deck; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deck
    ADD CONSTRAINT fk_userid_deck FOREIGN KEY (userid) REFERENCES public.users(id);


--
-- Name: usercards fk_userid_usercards; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.usercards
    ADD CONSTRAINT fk_userid_usercards FOREIGN KEY (userid) REFERENCES public.users(id);


--
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

GRANT USAGE ON SCHEMA public TO admin;


--
-- Name: TABLE actions; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.actions TO admin;


--
-- Name: TABLE battles; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.battles TO admin;


--
-- Name: TABLE cards; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.cards TO admin;


--
-- Name: TABLE deck; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.deck TO admin;


--
-- Name: TABLE elementtypes; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.elementtypes TO admin;


--
-- Name: TABLE users; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.users TO admin;


--
-- PostgreSQL database dump complete
--
