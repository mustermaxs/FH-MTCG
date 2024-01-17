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


--
-- Name: cardname; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.cardname AS ENUM (
    'WaterGoblin',
    'FireGoblin',
    'RegularGoblin',
    'WaterTroll',
    'FireTroll',
    'RegularTroll',
    'WaterElf',
    'FireElf',
    'RegularElf',
    'WaterSpell',
    'FireSpell',
    'RegularSpell',
    'Knight',
    'Dragon',
    'Ork',
    'Kraken'
);


ALTER TYPE public.cardname OWNER TO postgres;

--
-- Name: tradetype; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.tradetype AS ENUM (
    'store',
    'users'
);


ALTER TYPE public.tradetype OWNER TO postgres;

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
-- Name: sessions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sessions (
    id integer NOT NULL,
    token character varying(50) NOT NULL,
    userid uuid
);


ALTER TABLE public.sessions OWNER TO postgres;

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

ALTER SEQUENCE public.authtokes_id_seq OWNED BY public.sessions.id;


--
-- Name: battlelogs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.battlelogs (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    cardidplayer1 uuid NOT NULL,
    cardidplayer2 uuid NOT NULL,
    player1 uuid,
    player2 uuid NOT NULL,
    roundwinner uuid,
    battleid uuid NOT NULL,
    roundnumber smallint NOT NULL,
    isdraw boolean DEFAULT false
);


ALTER TABLE public.battlelogs OWNER TO postgres;

--
-- Name: battles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.battles (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    player1 uuid NOT NULL,
    player2 uuid NOT NULL,
    winner uuid,
    isdraw boolean,
    enddatetime timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    countrounds smallint,
    gainedpoints smallint,
    battletoken character varying(10) NOT NULL
);


ALTER TABLE public.battles OWNER TO postgres;

--
-- Name: cards; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cards (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    descr text,
    damage double precision NOT NULL,
    type character varying(50) NOT NULL,
    element character varying(50) NOT NULL,
    name character varying(50) DEFAULT ''::character varying NOT NULL,
    CONSTRAINT check_element_not_empty CHECK ((length((element)::text) > 0)),
    CONSTRAINT check_name_not_empty CHECK ((length((name)::text) > 0)),
    CONSTRAINT check_type_not_empty CHECK ((length((type)::text) > 0))
);


ALTER TABLE public.cards OWNER TO postgres;

--
-- Name: cardtrades; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cardtrades (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    offeringuserid uuid,
    acceptinguserid uuid,
    offeredcardid uuid,
    acceptedcardid uuid,
    minimumdamage double precision,
    requiredtype character varying(50),
    settled boolean DEFAULT false,
    deckid uuid
);


ALTER TABLE public.cardtrades OWNER TO postgres;

--
-- Name: deck; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.deck (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    cardid uuid,
    userid uuid,
    countcardsoftype smallint,
    locked boolean DEFAULT false
);


ALTER TABLE public.deck OWNER TO postgres;

--
-- Name: decks; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.decks (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    userid uuid,
    cardid uuid
);


ALTER TABLE public.decks OWNER TO postgres;

--
-- Name: elementtypes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.elementtypes (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    elementname character(50)
);


ALTER TABLE public.elementtypes OWNER TO postgres;

--
-- Name: packagecards; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.packagecards (
    packageid uuid NOT NULL,
    cardid uuid NOT NULL,
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL
);


ALTER TABLE public.packagecards OWNER TO postgres;

--
-- Name: packages; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.packages (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    price integer DEFAULT 5
);


ALTER TABLE public.packages OWNER TO postgres;

--
-- Name: roles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.roles (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    role character varying(15)
);


ALTER TABLE public.roles OWNER TO postgres;

--
-- Name: stackcards; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.stackcards (
    id uuid DEFAULT public.uuid_generate_v4(),
    cardid uuid,
    userid uuid
);


ALTER TABLE public.stackcards OWNER TO postgres;

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
    role uuid DEFAULT '49e2c62b-940f-4a58-a59a-e4fc8d7a69ff'::uuid,
    language character varying(20) DEFAULT 'english'::character varying,
    elo smallint DEFAULT 100 NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: sessions id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sessions ALTER COLUMN id SET DEFAULT nextval('public.authtokes_id_seq'::regclass);


--
-- Data for Name: actions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.actions (id, actiontype) FROM stdin;
\.


--
-- Data for Name: battlelogs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.battlelogs (id, cardidplayer1, cardidplayer2, player1, player2, roundwinner, battleid, roundnumber, isdraw) FROM stdin;
\.


--
-- Data for Name: battles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.battles (id, player1, player2, winner, isdraw, enddatetime, countrounds, gainedpoints, battletoken) FROM stdin;
\.


--
-- Data for Name: cards; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cards (id, descr, damage, type, element, name) FROM stdin;
\.


--
-- Data for Name: cardtrades; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cardtrades (id, offeringuserid, acceptinguserid, offeredcardid, acceptedcardid, minimumdamage, requiredtype, settled, deckid) FROM stdin;
\.


--
-- Data for Name: deck; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.deck (id, cardid, userid, countcardsoftype, locked) FROM stdin;
\.


--
-- Data for Name: decks; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.decks (id, userid, cardid) FROM stdin;
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
-- Data for Name: packagecards; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.packagecards (packageid, cardid, id) FROM stdin;
\.


--
-- Data for Name: packages; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.packages (id, price) FROM stdin;
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.roles (id, role) FROM stdin;
49e2c62b-940f-4a58-a59a-e4fc8d7a69ff	USER
8f5999cc-6542-4898-9eb1-587401412ae5	ADMIN
\.


--
-- Data for Name: sessions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.sessions (id, token, userid) FROM stdin;
\.


--
-- Data for Name: stackcards; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.stackcards (id, cardid, userid) FROM stdin;
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, name, password, coins, bio, image, role, language, elo) FROM stdin;
5eb99dbc-f562-4f9e-ba06-8b7b76d3dff2	test	9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08	20		:)	49e2c62b-940f-4a58-a59a-e4fc8d7a69ff	english	100
14076953-be7d-4bfb-9180-a797d9dad345	admin	8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918	20	mustermax-admin	:)	8f5999cc-6542-4898-9eb1-587401412ae5	english	100
060ab0c4-abef-4d89-bf45-c33af92a7c89	max	9baf3a40312f39849f46dad1040f2f039f1cffa1238c41e9db675315cfad39b6	20		:)	49e2c62b-940f-4a58-a59a-e4fc8d7a69ff	german	100
d324b594-06d6-42e2-b921-f371180edb8b	Wintersperger	f6bd083b0a55b6523a1348233369c5392d925f0d5cf5cbe7d270faa8600a865f	20	jazzhands	Brueckenmeier	49e2c62b-940f-4a58-a59a-e4fc8d7a69ff	english	100
5d53d909-dee4-44b9-886b-24b0654ee674	toni	bb37067afeb4ee16d668eef073ca6eea4f3b4a1fc6c68e3c0b1fd01a5fb7f5ad	20	miau	:)	49e2c62b-940f-4a58-a59a-e4fc8d7a69ff	english	100
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
-- Name: sessions authtokes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT authtokes_pkey PRIMARY KEY (id);


--
-- Name: battlelogs battlelog_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battlelogs
    ADD CONSTRAINT battlelog_pkey PRIMARY KEY (id);


--
-- Name: battles battles_battletoken_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT battles_battletoken_key UNIQUE (battletoken);


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
-- Name: decks decks_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.decks
    ADD CONSTRAINT decks_pkey PRIMARY KEY (id);


--
-- Name: decks decks_userid_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.decks
    ADD CONSTRAINT decks_userid_key UNIQUE (userid);


--
-- Name: elementtypes elementtypes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.elementtypes
    ADD CONSTRAINT elementtypes_pkey PRIMARY KEY (id);


--
-- Name: packagecards packagecards_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.packagecards
    ADD CONSTRAINT packagecards_pkey PRIMARY KEY (id);


--
-- Name: packages packages_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT packages_pkey PRIMARY KEY (id);


--
-- Name: roles roles_primary_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_primary_key PRIMARY KEY (id);


--
-- Name: cardtrades tradings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cardtrades
    ADD CONSTRAINT tradings_pkey PRIMARY KEY (id);


--
-- Name: cards unique_card; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cards
    ADD CONSTRAINT unique_card UNIQUE (descr, type, element, damage, name);


--
-- Name: battlelogs unique_row_prevent_race; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battlelogs
    ADD CONSTRAINT unique_row_prevent_race UNIQUE (roundnumber, battleid);


--
-- Name: sessions unique_userid; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sessions
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
-- Name: decks decks_userid_users; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.decks
    ADD CONSTRAINT decks_userid_users FOREIGN KEY (userid) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: cardtrades fk_acceptinguserid_users; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cardtrades
    ADD CONSTRAINT fk_acceptinguserid_users FOREIGN KEY (acceptinguserid) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: sessions fk_authtoken_userid; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT fk_authtoken_userid FOREIGN KEY (userid) REFERENCES public.users(id);


--
-- Name: battlelogs fk_battleid_battles; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battlelogs
    ADD CONSTRAINT fk_battleid_battles FOREIGN KEY (battleid) REFERENCES public.battles(id) ON DELETE CASCADE;


--
-- Name: packagecards fk_cardid_cards; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.packagecards
    ADD CONSTRAINT fk_cardid_cards FOREIGN KEY (cardid) REFERENCES public.cards(id) ON DELETE CASCADE;


--
-- Name: deck fk_cardid_cards; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deck
    ADD CONSTRAINT fk_cardid_cards FOREIGN KEY (cardid) REFERENCES public.cards(id) ON DELETE CASCADE;


--
-- Name: decks fk_cardid_stackcards; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.decks
    ADD CONSTRAINT fk_cardid_stackcards FOREIGN KEY (cardid) REFERENCES public.cards(id);


--
-- Name: battlelogs fk_cardidplayer1_cards; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battlelogs
    ADD CONSTRAINT fk_cardidplayer1_cards FOREIGN KEY (cardidplayer1) REFERENCES public.cards(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: battlelogs fk_cardidplayer2_cards; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battlelogs
    ADD CONSTRAINT fk_cardidplayer2_cards FOREIGN KEY (cardidplayer2) REFERENCES public.cards(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: cardtrades fk_deckid_decks; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cardtrades
    ADD CONSTRAINT fk_deckid_decks FOREIGN KEY (deckid) REFERENCES public.deck(id) ON DELETE CASCADE;


--
-- Name: cardtrades fk_offeredcardid_cards; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cardtrades
    ADD CONSTRAINT fk_offeredcardid_cards FOREIGN KEY (offeredcardid) REFERENCES public.cards(id) ON DELETE CASCADE;


--
-- Name: cardtrades fk_offeringuserid_users; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cardtrades
    ADD CONSTRAINT fk_offeringuserid_users FOREIGN KEY (offeringuserid) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: packagecards fk_packageid_packages; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.packagecards
    ADD CONSTRAINT fk_packageid_packages FOREIGN KEY (packageid) REFERENCES public.packages(id) ON DELETE CASCADE;


--
-- Name: battles fk_player1_battle; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT fk_player1_battle FOREIGN KEY (player1) REFERENCES public.users(id);


--
-- Name: battlelogs fk_player1_users; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battlelogs
    ADD CONSTRAINT fk_player1_users FOREIGN KEY (player1) REFERENCES public.users(id);


--
-- Name: battles fk_player2_battle; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT fk_player2_battle FOREIGN KEY (player2) REFERENCES public.users(id);


--
-- Name: battlelogs fk_player2_users; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battlelogs
    ADD CONSTRAINT fk_player2_users FOREIGN KEY (player2) REFERENCES public.users(id);


--
-- Name: users fk_roles; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT fk_roles FOREIGN KEY (role) REFERENCES public.roles(id);


--
-- Name: battlelogs fk_roundwinner_users; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battlelogs
    ADD CONSTRAINT fk_roundwinner_users FOREIGN KEY (roundwinner) REFERENCES public.users(id);


--
-- Name: stackcards fk_stackcards_userid; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.stackcards
    ADD CONSTRAINT fk_stackcards_userid FOREIGN KEY (userid) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: deck fk_userid_deck; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deck
    ADD CONSTRAINT fk_userid_deck FOREIGN KEY (userid) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: battles fk_winner_battle; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battles
    ADD CONSTRAINT fk_winner_battle FOREIGN KEY (winner) REFERENCES public.users(id);


--
-- Name: stackcards stackcards_cardid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.stackcards
    ADD CONSTRAINT stackcards_cardid_fk FOREIGN KEY (cardid) REFERENCES public.cards(id) ON DELETE CASCADE;


--
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

GRANT USAGE ON SCHEMA public TO admin;


--
-- Name: TABLE actions; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.actions TO admin;


--
-- Name: TABLE sessions; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.sessions TO admin;


--
-- Name: TABLE battlelogs; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.battlelogs TO admin;


--
-- Name: TABLE battles; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.battles TO admin;


--
-- Name: TABLE cards; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.cards TO admin;


--
-- Name: TABLE cardtrades; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.cardtrades TO admin;


--
-- Name: TABLE deck; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.deck TO admin;


--
-- Name: TABLE decks; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.decks TO admin;


--
-- Name: TABLE elementtypes; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.elementtypes TO admin;


--
-- Name: TABLE packagecards; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.packagecards TO admin;


--
-- Name: TABLE packages; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.packages TO admin;


--
-- Name: TABLE roles; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.roles TO admin;


--
-- Name: TABLE stackcards; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.stackcards TO admin;


--
-- Name: TABLE users; Type: ACL; Schema: public; Owner: postgres
--

GRANT ALL ON TABLE public.users TO admin;


--
-- PostgreSQL database dump complete
--

