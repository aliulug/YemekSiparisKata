using NSubstitute;
using NUnit.Framework;

namespace YemekSiparisUygulamasi
{
	[TestFixture]
	public class YemekSiparisMotoruTest
	{
		private IRestoranIletisimci _restoranIletisimci;
		private ISanalPos _sanalPos;
		private YemekSiparisMotoru _siparisMotoru;
		private SiparisBilgileri _odemesizSiparisBilgileri;
		private SiparisBilgileri _odemeliSiparisBilgileri;

		[SetUp]
		public void her_test_oncesi_calisacak_setup_metodu()
		{
			_restoranIletisimci = Substitute.For<IRestoranIletisimci>();
			_sanalPos = Substitute.For<ISanalPos>();
			_siparisMotoru = new YemekSiparisMotoru(_restoranIletisimci, _sanalPos);
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
	}

	public interface ISanalPos
	{
		void CekimYap(KrediKartBilgileri kartBilgileri, double tutar);
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
	}

	public class SiparisBilgileri
	{
		public SiparisOdemeTip OdemeTipi { get; set; }
		public KrediKartBilgileri KartBilgileri { get; set; }
		public double ToplamTutar { get; set; }
	}

	public class KrediKartBilgileri
	{
	}
}
