using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

namespace YemekSiparisUygulamasi
{
	[TestFixture]
	public class YemekSiparisMotoruTest
	{
		private IRestoranIletisimci _restoranIletisimci;
		private ISanalPos _sanalPos;
		private ISiparisRepo _siparisRepo;
		private ICallCenterIletisimci _callCenterIletisimci;
		private YemekSiparisMotoru _siparisMotoru;
		private SiparisBilgileri _odemesizSiparisBilgileri;
		private SiparisBilgileri _odemeliSiparisBilgileri;

		[SetUp]
		public void her_test_oncesi_calisacak_setup_metodu()
		{
			_restoranIletisimci = Substitute.For<IRestoranIletisimci>();
			_sanalPos = Substitute.For<ISanalPos>();
			_siparisRepo = Substitute.For<ISiparisRepo>();
			_callCenterIletisimci = Substitute.For<ICallCenterIletisimci>();
			_siparisMotoru = new YemekSiparisMotoru(_restoranIletisimci, _sanalPos, _siparisRepo, _callCenterIletisimci);
			_odemesizSiparisBilgileri = new SiparisBilgileri();
			_odemeliSiparisBilgileri = new SiparisBilgileri
			{
				OdemeTipi = SiparisOdemeTip.OnlineKrediKarti
			};
		}

		[Test]
		public void siparis_geldiginde_ilgili_restorana_yonlendirilir()
		{
			//when
			_siparisMotoru.SiparisVer(_odemesizSiparisBilgileri);

			//then
			_restoranIletisimci.Received().SiparisBilgileriniGonder(_odemesizSiparisBilgileri);
		}

		[Test]
		public void odeme_tipi_online_kart_ise_kart_cekimi_yapilir()
		{
			//when
			_siparisMotoru.SiparisVer(_odemeliSiparisBilgileri);

			//then
			_sanalPos.Received().CekimYap(_odemeliSiparisBilgileri.KartBilgileri, _odemeliSiparisBilgileri.ToplamTutar);
		}

		[Test]
		public void kart_cekimi_basarisiz_olursa_hata_doner_restorana_siparis_gecilmez()
		{
			_sanalPos
				.CekimYap(_odemeliSiparisBilgileri.KartBilgileri, _odemeliSiparisBilgileri.ToplamTutar)
				.Returns(false);

			SiparisSonuc siparisSonucu = _siparisMotoru.SiparisVer(_odemeliSiparisBilgileri);

			Assert.IsFalse(siparisSonucu.SiparisBasarilimi);
			_restoranIletisimci.DidNotReceive().SiparisBilgileriniGonder(_odemeliSiparisBilgileri);
		}

		[Test]
		public void kart_cekimi_basariliysa_basarili_cevap_doner_restorana_siparis_gecilir()
		{
			_sanalPos
				.CekimYap(_odemeliSiparisBilgileri.KartBilgileri, _odemeliSiparisBilgileri.ToplamTutar)
				.Returns(true);

			SiparisSonuc siparisSonucu = _siparisMotoru.SiparisVer(_odemeliSiparisBilgileri);

			Assert.IsTrue(siparisSonucu.SiparisBasarilimi);
			_restoranIletisimci.Received().SiparisBilgileriniGonder(_odemeliSiparisBilgileri);
		}

		[Test]
		public void siparis_veritabanina_kaydedilir()
		{
			_siparisMotoru.SiparisVer(_odemesizSiparisBilgileri);

			_siparisRepo.Received().VeritabaninaKaydet(_odemesizSiparisBilgileri);
		}

		[Test]
		public void kart_cekim_sonucu_veritabanina_kaydedilir()
		{
			_sanalPos
				.CekimYap(_odemeliSiparisBilgileri.KartBilgileri, _odemeliSiparisBilgileri.ToplamTutar)
				.Returns(true);

			_siparisMotoru.SiparisVer(_odemeliSiparisBilgileri);

			_siparisRepo.Received().SiparisCekimBilgisiGuncelle(_odemeliSiparisBilgileri, true);
		}

		[Test]
		public void cevapsiz_siparisler_bes_dakika_sonra_iptal_edilir()
		{
			List<SiparisBilgileri> cevapsizSiparisler = new List<SiparisBilgileri>
			{
				new SiparisBilgileri { SiparisTarihi = DateTime.Now },
				new SiparisBilgileri { SiparisTarihi = DateTime.Now.AddMinutes(-5) },
				new SiparisBilgileri { SiparisTarihi = DateTime.Now },
				new SiparisBilgileri { SiparisTarihi = DateTime.Now.AddMinutes(-6) },
				new SiparisBilgileri { SiparisTarihi = DateTime.Now },
			};
			_siparisRepo.CevapsizSiparisleriAl().Returns(cevapsizSiparisler);

			_siparisMotoru.BelliBirSuredirCevapAlinamayanSiparisleriIptalEt();

			_callCenterIletisimci.Received(2).SiparisIptalBilgisiIlet(Arg.Any<SiparisBilgileri>());
			_siparisRepo.Received(2).SiparisiIptalOlarakKaydet(Arg.Any<SiparisBilgileri>());
			_restoranIletisimci.Received(2).SiparisIptalIlet(Arg.Any<SiparisBilgileri>());
		}

		[Test]
		public void restoran_siparisi_onaylarsa_veritabanina_kaydet()
		{
			_siparisMotoru.RestoranCevabiIsle(_odemesizSiparisBilgileri, true);

			_siparisRepo.Received().SiparisiOnaylandiOlarakKaydet(_odemesizSiparisBilgileri);
		}

		[Test]
		public void restorans_siparisi_reddederse_veritabanina_kaydet_call_center_bilgi_ver()
		{
			_siparisMotoru.RestoranCevabiIsle(_odemesizSiparisBilgileri, false);

			_siparisRepo.Received().SiparisiIptalOlarakKaydet(_odemesizSiparisBilgileri);
			_callCenterIletisimci.SiparisIptalBilgisiIlet(_odemesizSiparisBilgileri);

		}
	}

	public interface ICallCenterIletisimci
	{
		void SiparisIptalBilgisiIlet(SiparisBilgileri siparisBilgileri);
	}

	public interface ISiparisRepo
	{
		void VeritabaninaKaydet(SiparisBilgileri siparisBilgileri);
		void SiparisCekimBilgisiGuncelle(SiparisBilgileri siparisBilgileri, bool kartCekimiBasarilimi);
		List<SiparisBilgileri> CevapsizSiparisleriAl();
		void SiparisiIptalOlarakKaydet(SiparisBilgileri siparisBilgileri);
		void SiparisiOnaylandiOlarakKaydet(SiparisBilgileri siparisBilgileri);
	}

	public class SiparisSonuc
	{
		public SiparisSonuc(bool siparisBasarilimi)
		{
			SiparisBasarilimi = siparisBasarilimi;
		}

		public bool SiparisBasarilimi { get; set; }
	}

	public interface ISanalPos
	{
		bool CekimYap(KrediKartBilgileri kartBilgileri, double tutar);
	}

	public enum SiparisOdemeTip
	{
		Nakit,
		KapidaKrediKarti,
		OnlineKrediKarti
	}

	public interface IRestoranIletisimci
	{
		void SiparisBilgileriniGonder(SiparisBilgileri siparisBilgileri);
		void SiparisIptalIlet(SiparisBilgileri siparisBilgileri);
	}

	public class SiparisBilgileri
	{
		public SiparisOdemeTip OdemeTipi { get; set; }
		public KrediKartBilgileri KartBilgileri { get; set; }
		public double ToplamTutar { get; set; }
		public DateTime SiparisTarihi { get; set; }
	}

	public class KrediKartBilgileri
	{
	}
}
