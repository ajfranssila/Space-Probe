using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

/// @author Antti Franssila
/// @version Space Probe 1.0
///
/// <summary>
/// Space Probe (suom. Avaruusluotain) on peli, jossa pelaajan tavoite on läpäistä tehtävät avaruusluotainta ohjaamalla.
/// </summary>
public class Avaruusluotain : PhysicsGame
{
    private static readonly string tarina = "\nThe new space program has started and your mission is to complete the tasks we give you. \nThe previous group failed to collect enough important data but we can use the landing zones they constructed. \nOur new probe has better and more expensive sensors so we hope that this time we can gather the data we need. \nThe probe has been built for years so be careful. There's no room for failure. Good luck!";
    private static readonly String[] alkuteksti = new string[]
    {
        "\nMission 1: Training camp\n\nPlanet: Earth, Gravity: Normal\n\nFuel: 700\n\nMission: Land on the landing zone\n",
        "\nMission 2: Small step for a probe\n\nPlanet: Moon, Gravity: Low\n\nFuel: 300 (Critical)\n\nMission: Land on the landing zone\n",
        "\nMission 3: Too close to the Sun\n\nPlanet: Mercury, Gravity: Quite low\n\nFuel: 1100\n\nMission: Bring back the mineral (how that ship got there?)\nBe quick or you'll burn yourself!\n",
        "\nMission 4: Tunnels of Venus\n\nPlanet: Venus, Gravity: Normal\n\nFuel: 3000\n\nMission: Land on the landing zone\n",
        "\nMission 5: Someone's been here\n\nPlanet: Mars, Gravity: Quite low\n\nFuel: 2150\n\nMission: Land on the landing zone\n",
        "\nMission 6: On the rock core\n\nPlanet: Jupiter, Gravity: High\n\nFuel: 2400\n\nMission: Land on the landing zone\n",
        "\nMission 7: The way back\n\nPlanet: No planet, just asteroids, Gravity: None\n\nFuel: 1500\n\nMission: Navigate through the asteroid cloud\n",
        "\nMission 8: Back to Earth\n\nPlanet: Earth, Gravity: Normal\n\nFuel: Only last drops left\n\nMission: Bring the probe back in one piece\n",
    };
    private static readonly Image maanKuva = LoadImage("MaaKuva"); // Kuvat ja äänet ladataan attribuutteina, jolloin ne ladataan vain kerran käynnistyksen yhteydessä ja kenttien vaihtuminen on sujuvampaa.
    private static readonly Image maastonKuva = LoadImage("Kentta1Kuva");
    private static readonly Image maaston2Kuva = LoadImage("KuunkuvaUusin2");
    private static readonly Image maaston4Kuva = LoadImage("VenusUusin");
    private static readonly Image maaston8Kuva = LoadImage("MaaViimeinen4");
    private static readonly Image maaston3Kuva = LoadImage("Merkurius");
    private static readonly Image maaston5Kuva = LoadImage("VuoretMaasto");
    private static readonly Image maaston6Kuva = LoadImage("jupiterUusi");
    private static readonly Image asteroidi1 = LoadImage("Asteroidi1");
    private static readonly Image asteroidi2 = LoadImage("Asteroidi2");
    private static readonly Image alustanKuva = LoadImage("alusta");
    private static readonly Image asema = LoadImage("Asema");
    private static readonly Image sukkulanKuva = LoadImage("Sukkula");
    private static readonly Image katonKuva = LoadImage("SukkulanKatto");
    private static readonly Image auringonKuva = LoadImage("Aurinko");
    private static readonly Image mineraalinKuva = LoadImage("mineraali");
    private static readonly Image luolanKuva = LoadImage("Luola2");
    private static readonly Image kivenKuva = LoadImage("kivi");
    private static readonly Image planeetankuva = LoadImage("PlaneettaRuskea");
    private static readonly Image pienenKivenKuva = LoadImage("pieniKivi");
    private static readonly Image esteKuva = LoadImage("este");
    private static readonly Image pelaajanKuva = LoadImage("Avaruusluotain");
    private static readonly Image pelaajanKuvaRikki = LoadImage("AvaruusluotainRikki");
    private static readonly SoundEffect rakettimoottori = LoadSoundEffect("rakettimoottori");
    private static readonly SoundEffect ovi = LoadSoundEffect("ovi");
    private static readonly SoundEffect tormays = LoadSoundEffect("tormays");
    private static readonly SoundEffect mustaAukko = LoadSoundEffect("mustaAukko");
    private static Sound rakettimoottoriKayta;
    private static Sound tormaysKayta;
    private static Sound mustaAukkoKayta;
    private static Animation moottori;
    private static Animation mustaAukkoAnimaatio;
    private static PhysicsObject laskeutumisAlusta; // Laskeutumisalusta on attribuuttina, jotta törmäyksenkäsittelijöiden luonti on helpompaa.  
    private static bool pelaajaOnTuhoutunut; // Pitää yllä tietoa siitä onko pelaaja tuhoutunut. Tarvitaan törmäyksen-/tapahtumankäsittelijöissä.
    private static bool pelaajaOnLaskeutunut; // Pitää yllä tietoa siitä onko pelaaja laskeutunut. Tarvitaan törmäyksen-/tapahtumankäsittelijöissä.
    private static bool ekaStartti = false; // Tarvitaan ensimmäisen pelikerran tarinatekstin luomista varten.
    private static int mikaKentta = 0; // Pitää yllä tietoa siitä, missä kentässä pelaaja on.
    private static int pelaajanTuhoutumiset = 0; // Pitää yllä lukemaa pelaajan tuhoutumisista. Tämä luku saatetaan pelaajan tietoon pelin päätteeksi.
    private static int resoluutioLeveys = 1600; // Pitää yllä asetuksissa asetettua resoluutiota kentän vaihtuessa tai vaikka peli aloitettaisiin asetuksien kautta alusta.
    private static int resoluutioKorkeus = 900;

    /// <summary>
    /// Pelin tosiasiallinen "pääohjelma", jossa luodaan pelimaailma ja valikot jokaista kenttää varten sekä asetetaan oletusarvoiset törmäyksenkäsittelijät ja arvot.
    /// Kentästä riippuen luodaan myös kentälle ominaiset arvot, oliot ja asetukset.
    /// </summary>
    public override void Begin()
    {
        SetWindowSize(resoluutioLeveys, resoluutioKorkeus);
        Level.Width = resoluutioLeveys;
        Level.Height = resoluutioKorkeus;
        Camera.ZoomToLevel();

        Label bensamittari = new Label();
        IntMeter bensanMaara = new IntMeter(500, 0, 10000);
        CollisionShapeParameters tormaysparametrit = new CollisionShapeParameters();
        tormaysparametrit.DistanceGridSpacing = 25; // Törmäysparametrien muuttaminen, jotta fysiikat törmäyksissä ovat tarkempia.
        tormaysparametrit.MaxVertexDistance = 25;
        PhysicsObject pelaaja = new PhysicsObject(50, 50);
        //moottori = LoadAnimation("Moottori");
        rakettimoottoriKayta = rakettimoottori.CreateSound(); // Muutetaan äänet Soundeffectistä Soundiksi, jotta saadaan oikeanlainen ääni.
        tormaysKayta = tormays.CreateSound();
        LuoOhjaimet(pelaaja, alkuteksti, bensamittari, bensanMaara);
        IsPaused = false;

        switch (mikaKentta)
        {
            case 0: // Case 0 tarkoitus on luoda ensimmäisellä käynnistyskerralla pelin aloitusteksti ennen pelin aloittamista.
                LuoAlkuvalikko(alkuteksti, bensamittari, pelaaja);
                LuoKentta(maastonKuva, -200, 700, tormaysparametrit, bensanMaara);
                Level.Background.CreateGradient(Color.White, Color.Azure);
                LuoLaskeutumisalusta(700, -176, 100);
                LuoPelaaja(pelaaja, -400, 0);
                mikaKentta++;
                break;
            case 1:
                LuoAlkuteksti(alkuteksti, pelaaja);
                LuoKentta(maastonKuva, -200, 700, tormaysparametrit, bensanMaara);
                Level.Background.CreateGradient(Color.White, Color.Azure);
                LuoLaskeutumisalusta(700, -176, 100);
                LuoPelaaja(pelaaja, -400, 0);
                break;
            case 2:
                LuoAlkuteksti(alkuteksti, pelaaja);
                LuoKentta(maaston2Kuva, -100, 300, tormaysparametrit, bensanMaara);
                Level.Background.CreateStars();
                LuoLaskeutumisalusta(210, -440, 100);
                LuoPelaaja(pelaaja, -700, -50);

                PhysicsObject maa = PhysicsObject.CreateStaticObject(400, 400);
                maa.CollisionIgnoreGroup = 1;
                maa.Image = maanKuva;
                maa.Position = new Vector(Level.Left + 400, Level.Top - 300);
                Add(maa, 0);
                break;
            case 3:
                LuoAlkuteksti(alkuteksti, pelaaja);

                LuoKentta(maaston3Kuva, -150, 1100, tormaysparametrit, bensanMaara);
                Level.Background.CreateStars();
                LuoPelaaja(pelaaja, -500, 25);
                PhysicsObject aurinko = PhysicsObject.CreateStaticObject(2000, 1100);
                aurinko.Image = auringonKuva;
                aurinko.Position = new Vector(-700, 500);
                aurinko.CollisionIgnoreGroup = 1;
                Add(aurinko, -1);

                //Shape sukkulanMuoto = Shape.FromImage(sukkulanKuva);
                PhysicsObject sukkula = new PhysicsObject(800, 400);
                sukkula.Angle = Angle.FromDegrees(-10);
                sukkula.Position = new Vector(-550, 80);
                sukkula.Image = sukkulanKuva;
                sukkula.CollisionIgnoreGroup = 2;
                Add(sukkula);

                PhysicsObject sukkulanKatto = new PhysicsObject(360, 60);
                sukkulanKatto.CollisionIgnoreGroup = 1;
                sukkulanKatto.IgnoresCollisionResponse = true;
                sukkula.IgnoresPhysicsLogics = true;
                sukkulanKatto.IgnoresGravity = true;
                sukkulanKatto.Angle = Angle.FromDegrees(-10);
                sukkulanKatto.Position = new Vector(sukkula.X - 10, sukkula.Y - 110);
                sukkulanKatto.Image = katonKuva;
                Add(sukkulanKatto, 1);

                LuoLaskeutumisalusta(sukkula.X, sukkula.Y - 76, 400, Color.Transparent);
                laskeutumisAlusta.CollisionIgnoreGroup = 1;
                laskeutumisAlusta.Color = Color.Transparent;
                laskeutumisAlusta.Angle = Angle.FromDegrees(-10);

                PhysicsObject mineraali = new PhysicsObject(20, 20);
                mineraali.Image = mineraalinKuva;
                //mineraali.Shape = Shape.FromImage(mineraalinKuva);
                mineraali.Position = new Vector(700, -321);
                Add(mineraali);

                AddCollisionHandler(pelaaja, mineraali, delegate (PhysicsObject luotain, PhysicsObject kohde) { laskeutumisAlusta.CollisionIgnoreGroup = 0; mineraali.Destroy(); pelaaja.Mass += 0.75; });

                Image kipina = LoadImage("Kipina");
                Flame liekki = new Flame(kipina);
                liekki.MinLifetime = -40;
                liekki.MaxLifetime = 2;
                liekki.Position = pelaaja.Position;
                FollowerBrain liekinAivo = new FollowerBrain(pelaaja);
                liekki.Brain = liekinAivo;
                liekinAivo.Speed = 10;
                liekki.MinScale = 0;
                liekki.MaxScale = 10;

                Timer merkuriusAjastin = new Timer();
                IntMeter merkuriusAika = new IntMeter(25, 0, 25);
                merkuriusAjastin.Interval = 1;
                merkuriusAjastin.Timeout += delegate { if (merkuriusAika == 22) Add(liekki); if (liekki.MinLifetime < 1.5) liekki.MinLifetime += 2; merkuriusAika.Value -= 1; };
                merkuriusAjastin.Start();
                merkuriusAika.LowerLimit += delegate { TuhoaPelaaja(pelaaja); };

                Label aikanaytto = new Label("moro");
                aikanaytto.TextColor = Color.White;
                aikanaytto.Position = new Vector(Screen.Left + 100, Screen.Top - 130);
                aikanaytto.BindTo(merkuriusAika);
                aikanaytto.IntFormatString = "Time: {0:D2}";
                Add(aikanaytto);

                Sound oviKayta = ovi.CreateSound();

                AddCollisionHandler(pelaaja, laskeutumisAlusta, delegate (PhysicsObject luotain, PhysicsObject laskeutumisAlusta) { if (pelaajaOnTuhoutunut == false) { merkuriusAjastin.Stop(); sukkulanKatto.MoveTo(new Vector(sukkula.X - 15, sukkula.Y - 47), 50); liekki.Destroy(); oviKayta.Play(); } });

                break;
            case 4:
                LuoAlkuteksti(alkuteksti, pelaaja);
                LuoKentta(maaston4Kuva, -200, 3000, tormaysparametrit, bensanMaara);
                Level.Background.Color = Color.Black;
                LuoLaskeutumisalusta(700, -370, 100);
                LuoPelaaja(pelaaja, Level.Left + 70, Level.Top - 200);
                Level.AmbientLight = 0.075;

                Light valo = new Light();
                valo.Intensity = 0.0;
                valo.Distance = 30;
                Add(valo);

                Timer aika = new Timer();
                aika.Interval = 0.1;

                Keyboard.Listen(Key.Up, ButtonState.Pressed, delegate { aika.Start(); aika.Timeout += delegate { valo.Position = pelaaja.Position; valo.Intensity = 1.0; }; }, "Valo, joka seuraa pelaajaa aina, kun rakettimoottoria käytetään");
                Keyboard.Listen(Key.Up, ButtonState.Released, delegate { aika.Stop(); valo.Intensity = 0.0; }, "Avaruusluotaimen rakettimoottorin animaation lopetus");
                AddCollisionHandler(pelaaja, delegate (PhysicsObject luotain, PhysicsObject kohde)
                {
                    const int NOPEUSRAJA = 100;
                    if (pelaaja.Velocity.Y > NOPEUSRAJA || pelaaja.Velocity.Y < -NOPEUSRAJA || pelaaja.Velocity.X > NOPEUSRAJA || pelaaja.Velocity.X < -NOPEUSRAJA)
                    {
                        aika.Start(); aika.Timeout += delegate { valo.Position = pelaaja.Position; }; valo.Intensity = 2.0;
                    }
                });
                break;
            case 5:
                Level.Width = 4800;
                Level.Height = 900;
                LuoKentta(maaston5Kuva, -150, 2150, tormaysparametrit, bensanMaara, 4800);
                Level.Background.CreateStars();
                LuoAlkuteksti(alkuteksti, pelaaja);
                LuoLaskeutumisalusta(2250, -357, 200);
                LuoPelaaja(pelaaja, 100, -150, true);

                PhysicsObject luola = PhysicsObject.CreateStaticObject(840, 510);
                luola.CollisionIgnoreGroup = 1;
                luola.Image = luolanKuva;
                luola.Position = new Vector(1247, 155);
                Add(luola, 1);

                PhysicsObject luolanSuu = PhysicsObject.CreateStaticObject(270, 30, Shape.Circle);
                luolanSuu.Image = Image.FromGradient(100, 10, Color.Black, Color.Charcoal);
                luolanSuu.CollisionIgnoreGroup = 1;
                luolanSuu.Angle = Angle.FromDegrees(5);
                luolanSuu.Position = new Vector(luola.X - 80, luola.Bottom + 150);
                Add(luolanSuu);

                PhysicsObject esteYlin = TeeEste(470, -138);
                esteYlin.Mass = 45;
                TeeEste(470, -240);
                TeeEste(470, -342);

                PhysicsObject kivi = new PhysicsObject(100, 100, Shape.Circle);
                kivi.Position = new Vector(-2000, 100);
                kivi.Image = kivenKuva;
                kivi.Mass = 10;
                Add(kivi);

                PhysicsObject planeetta = PhysicsObject.CreateStaticObject(400, 400);
                planeetta.CollisionIgnoreGroup = 1;
                planeetta.Image = planeetankuva;
                planeetta.AbsolutePosition = new Vector(Level.Right - 100, Level.Top - 100);
                Add(planeetta, 0);
                break;
            case 6:
                Keyboard.Clear();
                LuoOhjaimet(pelaaja, alkuteksti, bensamittari, bensanMaara, 450);
                LuoAlkuteksti(alkuteksti, pelaaja);
                LuoKentta(maaston6Kuva, -400, 2400, tormaysparametrit, bensanMaara);
                Level.Background.Image = LoadImage("JupiterTausta");
                Level.Background.CreateStars();
                LuoLaskeutumisalusta(700, -195, 70);
                LuoPelaaja(pelaaja, -700, 400);
                LopetaNappainkomennot();
                break;
            case 7:
                mustaAukkoKayta = mustaAukko.CreateSound();
                //mustaAukkoAnimaatio = LoadAnimation("MustanAukonAnimaatio");
                LuoAlkuteksti(alkuteksti, pelaaja);
                Level.Width = 8000;
                Level.CreateBorders();
                Level.Background.CreateStars();
                LuoLaskeutumisalusta(Level.Right, Level.Bottom, 1);
                LuoPelaaja(pelaaja, Level.Left + 100, 0, true);
                pelaajaOnTuhoutunut = false;
                pelaajaOnLaskeutunut = false;
                bensanMaara.Value = 1500;
                for (int i = 0; i < 15; i++)
                {
                    PhysicsObject a = LuoAsteroidit(200, 500);

                    if (Vector.Distance(a.Position, pelaaja.Position) < a.Height)
                    {
                        a.Destroy();
                        i--;
                    }
                }

                int[,] mustanAukonKohdat = new int[,] { { -2500, 300 }, { 0, -300 }, { 1500, 0 }, { 2500, 100 } };
                for (int i = 0; i < 4; i++)
                {
                    LuoMustaAukko(mustanAukonKohdat[i, 0], mustanAukonKohdat[i, 1], 5000, pelaaja);
                }

                Timer lapi = new Timer();
                lapi.Start();
                lapi.Interval = 0.1;
                lapi.Timeout += delegate
                {
                    if (pelaaja.X >= Level.Right - 100 && pelaajaOnTuhoutunut == false)
                    {
                        pelaaja.Image = pelaajanKuva;
                        LopetaNappainkomennot();
                        pelaajaOnLaskeutunut = true;
                        LuoKentanPaatosteksti();
                        Pause();
                    }
                };
                break;
            case 8:
                LuoAlkuteksti(alkuteksti, pelaaja);
                LuoKentta(maaston8Kuva, -210, 330, tormaysparametrit, bensanMaara);
                Level.Background.CreateGradient(Color.White, Color.Azure);
                LuoLaskeutumisalusta(677, -307, 200, Color.DarkGray);
                LuoPelaaja(pelaaja, -700, -100);

                PhysicsObject tukikohta = PhysicsObject.CreateStaticObject(200, 100);
                tukikohta.Position = new Vector(674, -255);
                tukikohta.Image = asema;
                tukikohta.CollisionIgnoreGroup = 1;
                Add(tukikohta);

                PhysicsObject pikkuKivi = PhysicsObject.CreateStaticObject(14, 14);
                pikkuKivi.Image = pienenKivenKuva;
                pikkuKivi.Position = new Vector(530, -300);
                Add(pikkuKivi);
                AddCollisionHandler(pelaaja, pikkuKivi, delegate (PhysicsObject luotain, PhysicsObject murikka) { pelaaja.AngularVelocity = -5; });
                break;
            case 9:
                Level.Background.Image = LoadImage("Loppu");
                LuoLaskeutumisalusta(700, -175, 100);
                LuoPelaaja(pelaaja, -400, 0);
                pelaaja.Destroy();
                laskeutumisAlusta.Destroy();
                bensamittari.Destroy();
                rakettimoottoriKayta.Volume = 0;

                string[] lopputekstit = new string[] { "Congratulations, Mission Accomplished", "You destroyed " + pelaajanTuhoutumiset + " probes during your journey",
                "The managers of the space program wish you good retirement", "They really hope you retire", "", "Credits:", "Game created by Antti Franssila", "",
                "Licence free stuff:", "Space probe image - historicspacecraft.com", "Images used in creating levels - pixabay.com", "Black hole animation - bestanimations.com", "Sounds - freesound.org" };

                Timer tekstiAjastin = new Timer();
                tekstiAjastin.Interval = 3;
                tekstiAjastin.Start();
                int tekstilaskuri = 0;
                tekstiAjastin.Timeout += delegate {
                    Label lopputeksti = new Label(lopputekstit[tekstilaskuri]);
                    lopputeksti.TextColor = Color.White;
                    lopputeksti.Y = -200;
                    lopputeksti.X = 30;
                    Add(lopputeksti);
                    lopputeksti.MoveTo(new Vector(30, 10), 10, lopputeksti.Destroy);
                    tekstilaskuri++;
                    if (tekstilaskuri == lopputekstit.Length) tekstiAjastin.Stop();
                };
                break;
        }
        LuoBensa(bensamittari, bensanMaara, pelaaja);
        AddCollisionHandler(pelaaja, Tuhoutuminen);
        AddCollisionHandler(pelaaja, tormaaminen);
        AddCollisionHandler(pelaaja, laskeutumisAlusta, Laskeutuminen);
    }

    /// <summary>
    /// Funktio palauttaa esteen hauttuun kohtaan.
    /// </summary>
    /// <param name="x">x-koordinaatti</param>
    /// <param name="y">y-koordinaatti</param>
    /// <returns>este</returns>
    private PhysicsObject TeeEste(int x, int y)
    {
        //Shape esteenMuoto = Shape.FromImage(esteKuva);
        PhysicsObject este = new PhysicsObject(100, 102);
        este.Image = esteKuva;
        este.X = x;
        este.Y = y;
        Add(este);
        return este;
    }

    /// <summary>
    /// Funktio luo ja palauttaa kooltaan annetulta kokoväliltä olevan asteroidin.
    /// </summary>
    /// <param name="kokoMin">Minimi koko</param>
    /// <param name="kokoMax">Maksimi koko</param>
    /// <returns>asteroidi</returns>
    private PhysicsObject LuoAsteroidit(int kokoMin, int kokoMax)
    {
        var asteroidi = new PhysicsObject(RandomGen.NextInt(kokoMin, kokoMax), RandomGen.NextInt(kokoMin, kokoMax));
        asteroidi.Position = Level.GetRandomPosition();
        asteroidi.AngularVelocity = RandomGen.NextDouble(-0.05, 0.05);
        asteroidi.Hit(RandomGen.NextVector(-5, -5, 5, 5));
        asteroidi.Mass = 100;

        Image[] asteroidienKuvat = new Image[2] { asteroidi1, asteroidi2 };
        int asteroidinKuva = RandomGen.NextInt(0, 2);
        asteroidi.Image = asteroidienKuvat[asteroidinKuva];
        Add(asteroidi);
        return asteroidi;
    }

    /// <summary>
    /// Aliohjelma luo annetulla voimalla pelaajaa puoleensa vetävän mustanaukon annettuun paikkaan.
    /// </summary>
    /// <param name="x">x-koordinaatti</param>
    /// <param name="y">y-koordinaatti</param>
    /// <param name="vetovoima">mustanaukon vetovoima</param>
    /// <param name="pelaaja">pelaaja</param>
    private void LuoMustaAukko(int x, int y, int vetovoima, PhysicsObject pelaaja)
    {
        PhysicsObject mustaAukko = PhysicsObject.CreateStaticObject(140, 140);
        mustaAukko.Position = new Vector(x, y);
        mustaAukko.Animation = new Animation(mustaAukkoAnimaatio);
        mustaAukko.Animation.Start();
        Add(mustaAukko, -1);
        mustaAukko.Tag = "mustaAukko";
        mustaAukko.CollisionIgnoreGroup = 1;
        Timer aukonAika = new Timer();
        aukonAika.Interval = 0.1;
        aukonAika.Start();
        aukonAika.Timeout += delegate { pelaaja.Hit(Vector.FromLengthAndAngle(vetovoima / Vector.Distance(pelaaja.Position, mustaAukko.Position), LuoMustanAukonSuunta(mustaAukko, pelaaja))); };

        Timer etaisyys = new Timer();
        etaisyys.Start();
        etaisyys.Interval = 0.1;
        etaisyys.Timeout += delegate
        {
            if (Vector.Distance(pelaaja.Position, mustaAukko.Position) < 50)
            {
                pelaaja.Destroy();
                mustaAukkoKayta.Play();
                etaisyys.Stop();
                if (pelaajaOnTuhoutunut == false) pelaajanTuhoutumiset++;
                Timer.SingleShot(3, LuoTuhoteksti);
            }
        };
    }

    /// <summary>
    /// Funktio palauttaa mustanaukon suunnan, johon pelaajaa "imetään".
    /// </summary>
    /// <param name="mustaAukko">musta-aukko</param>
    /// <returns>kulma, johon pelaajaa "imetään"</returns>
    private Angle LuoMustanAukonSuunta(PhysicsObject mustaAukko, PhysicsObject pelaaja)
    {
        if (pelaaja.X > mustaAukko.X) return (Angle.ArcTan((pelaaja.Y - mustaAukko.Y) / (pelaaja.X - mustaAukko.X)) + Angle.StraightAngle);
        else return (Angle.ArcTan((pelaaja.Y - mustaAukko.Y) / (pelaaja.X - mustaAukko.X)));
    }

    /// <summary>
    /// Luo alkutekstin pelin ensimmäisellä käynnistyskerralla. Tämä teksti luodaan siis vain kerran pelin aikana.
    /// </summary>
    /// <param name="alkuteksti">alkuteksti</param>
    /// <param name="pelaaja">pelaaja</param>
    private void LuoTarina(String[] alkuteksti, PhysicsObject pelaaja)
    {
        const string NAPPULA = "Continue...";
        MultiSelectWindow nappula = new MultiSelectWindow(tarina, NAPPULA);
        nappula.BorderColor = Color.Black;
        Add(nappula);
        IsPaused = true;
        nappula.AddItemHandler(0, delegate { LuoAlkuteksti(alkuteksti, pelaaja); });
    }

    /// <summary>
    /// Luo jokaisen kentän alkuun kentän aloitustekstin.
    /// </summary>
    /// <param name="alkuteksti">alkuteksti</param>
    /// <param name="pelaaja">pelaaja</param>
    private void LuoAlkuteksti(String[] alkuteksti, PhysicsObject pelaaja)
    {
        const string NAPPULANTEKSTI = "Ok, let's go!";
        MultiSelectWindow nappula = new MultiSelectWindow(alkuteksti[mikaKentta - 1], NAPPULANTEKSTI);
        nappula.BorderColor = Color.Black;
        Add(nappula);
        IsPaused = true;
        nappula.AddItemHandler(0, delegate { if (Keyboard.GetKeyState(Key.Up) == ButtonState.Down) { pelaaja.Animation = new Animation(moottori); pelaaja.Animation.Start(); } Pause(); });
    }

    /// <summary>
    /// Luo tekstin, joka näytetään, kun pelaaja tuhoutuu.
    /// </summary>
    private void LuoTuhoteksti()
    {
        const string TUHOTEKSTI = "\nObviously you can't handle the probe. Sadly you're our only hope.\n\nReady to go again?";
        const string NAPPULAN_TEKSTI = "Ready and steady";
        MultiSelectWindow nappula = new MultiSelectWindow(TUHOTEKSTI, NAPPULAN_TEKSTI);
        nappula.BorderColor = Color.Black;
        Add(nappula);
        nappula.AddItemHandler(0, AloitaAlusta);
    }

    /// <summary>
    /// Luo tekstin, joka näytetään, kun pelaaja pääsee kentän läpi. Napinpainalluksella siirrytään seuraavaan kenttään.
    /// </summary>
    private void LuoKentanPaatosteksti()
    {
        const string KENTAN_PAATOSTEKSTI = "\nGood job! Are you ready for the next planet?";
        const string KENTAN_PAATOSTEKSTI_VIIMEINEN = "\n\n It seems we made it. Finally... ";
        const string AFFIRMATIVE = "Affirmative";
        const string CONTINUE = "Continue";

        MultiSelectWindow nappula = new MultiSelectWindow(KENTAN_PAATOSTEKSTI, AFFIRMATIVE);
        if (mikaKentta == 8) { nappula = new MultiSelectWindow(KENTAN_PAATOSTEKSTI_VIIMEINEN, CONTINUE); }
        nappula.BorderColor = Color.Black;
        Add(nappula);
        nappula.AddItemHandler(0, delegate { mikaKentta++; AloitaAlusta(); });
    }

    /// <summary>
    /// Aliohjelma luo pelikentän.
    /// </summary>
    ///< param name="kentanKuva">Mistä kuvasta kenttä piirretään</param>
    ///<param name = "painovoima">Kentän painovoima</param>
    ///<param name = "paljonkoBensaa">Pelaajan bensamäärä kentän alussa</param>
    /// <param name="tormaysparametrit">törmäysparametrit</param>
    /// <param name="bensanMaara">intmeter jota kuljetetaan mukana bensanmäärän asettamiseksi</param>
    /// <param name="leveys">kentän leveys</param>
    private void LuoKentta(Image kentanKuva, double painovoima, int paljonkoBensaa, CollisionShapeParameters tormaysparametrit, IntMeter bensanMaara, int leveys = 1600)
    {
        Level.CreateBorders();
        pelaajaOnTuhoutunut = false;
        pelaajaOnLaskeutunut = false;
        //Shape maastonMuoto;
        //maastonMuoto = Shape.FromImage(kentanKuva);
        PhysicsObject maasto = PhysicsObject.CreateStaticObject(leveys, 900);
        maasto.Image = kentanKuva;
        Add(maasto);
        Gravity = new Vector(0, painovoima);
        bensanMaara.Value = paljonkoBensaa;
    }

    /// <summary>
    /// Aliohjelma luo alkuvalikon.
    /// </summary>
    /// <param name="alkuteksti">alkuteksti</param>
    /// <param name="bensamittari">bensamittari</param>
    /// <param name="pelaaja">pelaaja</param>
    private void LuoAlkuvalikko(String[] alkuteksti, Label bensamittari, PhysicsObject pelaaja)
    {
        IsPaused = true;
        rakettimoottoriKayta.Stop();
        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Space Probe v.1.0", "Play", "Settings", "Restart level", "Restart Game", "Quit");
        alkuvalikko.BorderColor = Color.Black;
        Add(alkuvalikko);
        alkuvalikko.AddItemHandler(0, Pause);
        alkuvalikko.AddItemHandler(0, delegate { if (Keyboard.GetKeyState(Key.Up) != ButtonState.Down) pelaaja.Image = pelaajanKuva; if (ekaStartti == false) { LuoTarina(alkuteksti, pelaaja); ekaStartti = true; } });
        alkuvalikko.AddItemHandler(1, delegate { Asetukset(alkuteksti, bensamittari, pelaaja); });
        alkuvalikko.AddItemHandler(2, AloitaAlusta);
        alkuvalikko.AddItemHandler(3, AloitaPeliAlusta);
        alkuvalikko.AddItemHandler(4, ConfirmExit);
    }

    /// <summary>
    /// Aliohjelma luo asetukset-valikon.
    /// </summary>
    /// <param name="alkuteksti">alkuteksti</param>
    /// <param name="bensamittari">bensamittari</param>
    /// <param name="pelaaja">pelaaja</param>
    private void Asetukset(String[] alkuteksti, Label bensamittari, PhysicsObject pelaaja)
    {
        MultiSelectWindow asetukset = new MultiSelectWindow("Settings", "Resolution", "Back");
        asetukset.BorderColor = Color.Black;
        Add(asetukset);
        asetukset.DefaultCancel = 1;
        asetukset.AddItemHandler(0, delegate { Resoluutio(alkuteksti, bensamittari, pelaaja); });
        asetukset.AddItemHandler(1, delegate { LuoAlkuvalikko(alkuteksti, bensamittari, pelaaja); });

    }

    /// <summary>
    /// Aliohjelma luo resoluutiovalikon, jossa voi vaihtaa pelin resoluutiota ja kytkeä kokoruututilan päälle.
    /// </summary>
    /// <param name="alkuteksti">alkuteksti</param>
    /// <param name="bensamittari">bensamittari</param>
    /// <param name="pelaaja">pelaaja</param>
    private void Resoluutio(String[] alkuteksti, Label bensamittari, PhysicsObject pelaaja)
    {
        MultiSelectWindow resoluutio = new MultiSelectWindow("Resolution", "Fullscreen On/Off", "1280x720", "1600x900", "1920x1080", "Back");
        resoluutio.BorderColor = Color.Black;
        Add(resoluutio);
        resoluutio.DefaultCancel = 4;
        resoluutio.ItemSelected += delegate (int valinta)
        {
            switch (valinta)
            {
                case 0:
                    if (IsFullScreen == false) IsFullScreen = true;
                    else IsFullScreen = false;
                    Resoluutio(alkuteksti, bensamittari, pelaaja);
                    break;
                case 1:
                    IsFullScreen = false;
                    resoluutioLeveys = 1280;
                    resoluutioKorkeus = 720;
                    SetWindowSize(resoluutioLeveys, resoluutioKorkeus); Camera.ZoomToLevel();
                    bensamittari.X = Screen.Left + 100;
                    bensamittari.Y = Screen.Top - 100;
                    Resoluutio(alkuteksti, bensamittari, pelaaja);
                    break;
                case 2:
                    IsFullScreen = false;
                    resoluutioLeveys = 1600;
                    resoluutioKorkeus = 900;
                    SetWindowSize(resoluutioLeveys, resoluutioKorkeus); Camera.ZoomToLevel();
                    bensamittari.X = Screen.Left + 100;
                    bensamittari.Y = Screen.Top - 100;
                    Resoluutio(alkuteksti, bensamittari, pelaaja);
                    break;
                case 3:
                    IsFullScreen = false;
                    resoluutioLeveys = 1920;
                    resoluutioKorkeus = 1080;
                    SetWindowSize(resoluutioLeveys, resoluutioKorkeus); Camera.ZoomToLevel();
                    bensamittari.X = Screen.Left + 100;
                    bensamittari.Y = Screen.Top - 100;
                    Resoluutio(alkuteksti, bensamittari, pelaaja);
                    break;
                case 4:
                    Asetukset(alkuteksti, bensamittari, pelaaja);
                    break;
            }
        };
    }

    ///<summary>
    ///Aliohjelma siirtää pelaajan parametreina annettuun kohtaan pelikenttää.
    ///</summary>
    ///<param name = "peli" ></ param >
    ///< param name="x">Pelaajan sijainti x-koordinaattina</param>
    ///<param name = "y" > Pelaajan sijainti y-koordinaattina</param>
    /// <param name="seuraakoKamera">Seuraako kamera pelaajaa</param>
    private void LuoPelaaja(PhysicsObject pelaaja, double x, double y, bool seuraakoKamera = false)
    {
        pelaaja.X = x;
        pelaaja.Y = y;
        pelaaja.Image = pelaajanKuva;
        pelaaja.AngularDamping = 0.95;
        pelaaja.CollisionIgnoreGroup = 1;
        Add(pelaaja, 1);
        if (seuraakoKamera == true) { Camera.Follow(pelaaja); Camera.StayInLevel = true; };
    }

    /// <summary>
    /// Aliohjelma luo laskeutumisalustan parametreina annettuun kohtaan pelikenttää. Myös Laskeutumisalustan leveys tulee antaa parametrina.
    /// </summary>
    /// <param name="x">Laskeutumisalustan sijainti x-koordinaattina</param>
    /// <param name="y">Laskeutumisalustan sijainti y-koordinaattina</param>
    /// <param name="leveys">Laskeutumisalustan leveys</param>
    /// <param name="vari">Laskeutumisalustan väri</param>
    private void LuoLaskeutumisalusta(double x, double y, double leveys, Color vari)
    {
        laskeutumisAlusta = PhysicsObject.CreateStaticObject(leveys, 5);
        laskeutumisAlusta.X = x;
        laskeutumisAlusta.Y = y;
        laskeutumisAlusta.Color = vari;
        Add(laskeutumisAlusta, 1);
    }

    /// <summary>
    /// Aliohjelma luo laskeutumisalustan parametreina annettuun kohtaan pelikenttää. Myös Laskeutumisalustan leveys tulee antaa parametrina.
    /// </summary>
    /// <param name="peli"></param>
    /// <param name="x">Laskeutumisalustan sijainti x-koordinaattina</param>
    /// <param name="y">Laskeutumisalustan sijainti y-koordinaattina</param>
    /// <param name="leveys">Laskeutumisalustan leveys</param>
    private void LuoLaskeutumisalusta(double x, double y, double leveys)
    {
        LuoLaskeutumisalusta(x, y, leveys, Color.Gray);
        laskeutumisAlusta.Image = alustanKuva;
    }

    /// <summary>
    /// Aliohjelma määrittelee napit, joilla avaruusalusta ohjataan ja asettaa "kaasunpainamiseen" äänet ja animaation.
    /// </summary>
    /// <param name="pelaaja">pelaaja</param>
    /// <param name="alkuteksti">alkuteksti</param>
    /// <param name="bensamittari">bensamittari</param>
    /// <param name="bensanMaara">Intmeter bensanmäärästä</param>
    /// <param name="kaasunVoima">rakettimoottorin teho eli työntävän voiman suuruus</param>
    private void LuoOhjaimet(PhysicsObject pelaaja, String[] alkuteksti, Label bensamittari, IntMeter bensanMaara, int kaasunVoima = 300)
    {
        Keyboard.Listen(Key.Up, ButtonState.Down, delegate { pelaaja.Push(Vector.FromLengthAndAngle(kaasunVoima, pelaaja.Angle + Angle.RightAngle)); bensanMaara.Value -= 1; }, "Avaruusluotaimen kaasu, joka työntää pelaajaa katon suuntaan");
        Keyboard.Listen(Key.Up, ButtonState.Pressed, delegate { pelaaja.Animation = new Animation(moottori); pelaaja.Animation.Start(); }, "Avaruusluotaimen rakettimoottorin animaatio");
        Keyboard.Listen(Key.Up, ButtonState.Released, delegate { pelaaja.Image = pelaajanKuva; }, "Avaruusluotaimen rakettimoottorin animaation lopetus");
        Keyboard.Listen(Key.Right, ButtonState.Down, delegate { pelaaja.ApplyTorque(-1000); }, "Kallistaa avaruusluotainta oikealle");
        Keyboard.Listen(Key.Left, ButtonState.Down, delegate { pelaaja.ApplyTorque(1000); }, "Kallistaa avaruusluotainta vasemmalle");
        Keyboard.Listen(Key.Up, ButtonState.Down, delegate { rakettimoottoriKayta.Play(); }, "Rakettimoottorin ääni");
        Keyboard.Listen(Key.Up, ButtonState.Released, delegate { rakettimoottoriKayta.Stop(); }, "Rakettimoottorin ääni, stop");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, LuoAlkuvalikko, "Takaisin alkuvalikkoon", alkuteksti, bensamittari, pelaaja);
    }

    /// <summary>
    /// Aliohjelma käsittelee pelaajan "törmäyksen" laskeutumisalustaan.
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="laskeutumisAlusta">laskeutumisalusta</param>
    private void Laskeutuminen(PhysicsObject pelaaja, PhysicsObject laskeutumisAlusta)
    {
        if (pelaaja.Velocity.Y < 100 && pelaaja.Velocity.Y > -100 && pelaaja.Velocity.X < 100 && pelaaja.Velocity.X > -100 && pelaajaOnTuhoutunut == false)
        {
            pelaaja.Image = pelaajanKuva;
            LopetaNappainkomennot();
            pelaajaOnLaskeutunut = true;
            LuoKentanPaatosteksti();
        }
    }

    /// <summary>
    /// Aliohjelma soittaa törmäysäänen, kun pelaaja törmää tarpeeksi kovalla vauhdilla esteisiin..
    /// </summary>
    /// <param name="pelaaja">pelaaja</param>
    /// <param name="kohde">ei käytössä</param>
    private void tormaaminen(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        const int NOPEUSRAJA = 50;
        if (Nopeusehto(pelaaja, NOPEUSRAJA) == true)
            tormaysKayta.Play();
    }

    /// <summary>
    /// Aliohjelma käsittelee pelaajan tuhoutumisen, kun pelaaja törmää liian kovalla vauhdilla esteisiin tai laskeutumisalueelle.
    /// </summary>
    /// <param name="pelaaja">pelaaja</param>
    /// <param name="kohde">ei käytössä</param>
    private void Tuhoutuminen(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        const int NOPEUSRAJA = 100;
        if (Nopeusehto(pelaaja, NOPEUSRAJA) == true)
        {
            TuhoaPelaaja(pelaaja);
        }
    }

    /// <summary>
    /// Laskee tuleeko nopeusehto täyteen, kun aliohjelmaa kutsutaan.
    /// </summary>
    /// <param name="pelaaja">pelaaja</param>
    /// <param name="nopeusraja">mitä nopeutta tarkastellaan</param>
    /// <returns>true/false</returns>
    private bool Nopeusehto(PhysicsObject pelaaja, int nopeusraja)
    {
        bool liianKovaa = false;
        if (pelaaja.Velocity.Y > nopeusraja || pelaaja.Velocity.Y < -nopeusraja || pelaaja.Velocity.X > nopeusraja || pelaaja.Velocity.X < -nopeusraja) liianKovaa = true;
        return liianKovaa;
    }

    /// <summary>
    /// Aliohjelma "tuhoaa" pelaajan poistamalla tältä kontrollit, vaihtamalla tekstuurin ja asettamalla efektit.
    /// </summary>
    /// <param name="pelaaja">pelaaja</param>
    private void TuhoaPelaaja(PhysicsObject pelaaja)
    {
        pelaaja.IgnoresExplosions = true;
        if (pelaajaOnTuhoutunut == false)
        {
            Explosion rajahdys = new Explosion(50);
            rajahdys.Position = pelaaja.Position;
            Add(rajahdys);
            pelaajanTuhoutumiset++;
        }
        pelaajaOnTuhoutunut = true;
        pelaaja.Image = pelaajanKuvaRikki;
        LopetaNappainkomennot();

        Smoke savu = new Smoke();
        savu.Position = pelaaja.Position;
        Add(savu, 3);
        savu.Tag = "savu";
        FollowerBrain aivo = new FollowerBrain(pelaaja);
        savu.Brain = aivo;
        aivo.Speed = 10;
        pelaaja.Destroyed += savu.Destroy;
        Timer.SingleShot(3, LuoTuhoteksti);
    }

    /// <summary>
    /// Aliohjelma poistaa ohjainkomennot avaruusluotaimelta.
    /// </summary>
    private void LopetaNappainkomennot()
    {
        Keyboard.Disable(Key.Up);
        Keyboard.Disable(Key.Left);
        Keyboard.Disable(Key.Right);
        rakettimoottoriKayta.Stop();
    }

    /// <summary>
    /// Aliohjelma luo bensamittarin, ja lopettaa ohjainkomennot bensan loppuessa.
    /// </summary>
    /// <param name="bensamittari">bensamittari</param>
    /// <param name="bensanMaara">intmeter bensanmäärästä</param>
    /// <param name="pelaaja">pelaaja</param>
    private void LuoBensa(Label bensamittari, IntMeter bensanMaara, PhysicsObject pelaaja)
    {
        bensamittari.X = Screen.Left + 100;
        bensamittari.Y = Screen.Top - 100;
        bensamittari.TextColor = Color.White;
        bensamittari.Color = Color.Transparent;
        bensamittari.BindTo(bensanMaara);
        bensamittari.IntFormatString = "Fuel: {0:D1}";
        Add(bensamittari);
        bensanMaara.LowerLimit += delegate { LopetaNappainkomennot(); pelaaja.Image = pelaajanKuva; Timer.SingleShot(7, delegate { if (pelaajaOnLaskeutunut == false) LuoTuhoteksti(); }); };
    }

    /// <summary>
    /// Aloittaa pelin alusta.
    /// </summary>
    private void AloitaPeliAlusta()
    {
        mikaKentta = 0;
        ekaStartti = false;
        pelaajanTuhoutumiset = 0;
        ClearAll();
        Begin();
    }

    /// <summary>
    /// Aloittaa meneillään olevan kentän alusta.
    /// </summary>
    private void AloitaAlusta()
    {
        ClearAll();
        Begin();
    }

    public void Pause()
    {
        IsPaused = !IsPaused;
    }
}

