using System;
using System.Linq;

namespace YemekSiparisUygulamasi
{
	public class YemekSiparisMotoru
	{
		private readonly IRestoranIletisimci _restoranIletisimci;
		private readonly ISanalPos _sanalPos;
		private readonly ISiparisRepo _siparisRepo;
		private readonly ICallCenterIletisimci _callCenterIletisimci;

		public YemekSiparisMotoru(IRestoranIletisimci restoranIletisimci, ISanalPos sanalPos, ISiparisRepo siparisRepo, ICallCenterIletisimci callCenterIletisimci)
		{
			_restoranIletisimci = restoranIletisimci;
			_sanalPos = sanalPos;
			_siparisRepo = siparisRepo;
			_callCenterIletisimci = callCenterIletisimci;
		}

		public SiparisSonuc SiparisVer(SiparisBilgileri siparisBilgileri)
		{
			_siparisRepo.VeritabaninaKaydet(siparisBilgileri);

			if (siparisBilgileri.OdemeTipi == SiparisOdemeTip.OnlineKrediKarti)
			{
				bool kartCekimiBasarili =_sanalPos.CekimYap(siparisBilgileri.KartBilgileri, siparisBilgileri.ToplamTutar);
				_siparisRepo.SiparisCekimBilgisiGuncelle(siparisBilgileri, kartCekimiBasarili);
				if (!kartCekimiBasarili)
					return new SiparisSonuc(false);
			}
			_restoranIletisimci.SiparisBilgileriniGonder(siparisBilgileri);
			return new SiparisSonuc(true);
		}

		public void BelliBirSuredirCevapAlinamayanSiparisleriIptalEt()
		{
			var besDakikadanFazlaBekleyenSiparisler = _siparisRepo.CevapsizSiparisleriAl()
				.Where(siparis => DateTime.Now.Subtract(siparis.SiparisTarihi).TotalMinutes >= 5);
			foreach (SiparisBilgileri siparis in besDakikadanFazlaBekleyenSiparisler)
			{
				_siparisRepo.SiparisiIptalOlarakKaydet(siparis);
				_callCenterIletisimci.SiparisIptalBilgisiIlet(siparis);
				_restoranIletisimci.SiparisIptalIlet(siparis);
			}
		}

		public void RestoranCevabiIsle(SiparisBilgileri siparisBilgileri, bool siparisOnaylandimi)
		{
			if (siparisOnaylandimi)
				_siparisRepo.SiparisiOnaylandiOlarakKaydet(siparisBilgileri);
			else
			{
				_siparisRepo.SiparisiIptalOlarakKaydet(siparisBilgileri);
				_callCenterIletisimci.SiparisIptalBilgisiIlet(siparisBilgileri);
			}
		}
	}
}