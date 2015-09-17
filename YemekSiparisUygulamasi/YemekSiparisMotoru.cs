namespace YemekSiparisUygulamasi
{
	public class YemekSiparisMotoru
	{
		private readonly IRestoranIletisimci _restoranIletisimci;
		private readonly ISanalPos _sanalPos;

		public YemekSiparisMotoru(IRestoranIletisimci restoranIletisimci, ISanalPos sanalPos)
		{
			_restoranIletisimci = restoranIletisimci;
			_sanalPos = sanalPos;
		}

		public void SiparisVer(SiparisBilgileri siparisBilgileri)
		{
			if (siparisBilgileri.OdemeTipi == SiparisOdemeTip.OnlineKrediKarti)
				_sanalPos.CekimYap(siparisBilgileri.KartBilgileri, siparisBilgileri.ToplamTutar);
			_restoranIletisimci.SiparisBilgileriniGonder(siparisBilgileri);
        }
	}
}