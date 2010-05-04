// Email addy script by Echostorm
// User may type /email <addy> in game to append their
// email to their entry in the Account DB.

using System;
using System.Text.RegularExpressions;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[Cmd(
		"&email",
		ePrivLevel.Player,
		"Set e-mail address for current account",
		"/email <address>")]
	public class EmailCommand : AbstractCommandHandler, ICommandHandler
	{
		#region ICommandHandler Members

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				client.Out.SendMessage("Usage: /email <address>",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				return;
			}
			EmailSyntaxValidator emailsyntaxvalidator; // validate mail
			string EmailAddy = args[1];
			emailsyntaxvalidator = new EmailSyntaxValidator(EmailAddy, true);
			if (!emailsyntaxvalidator.IsValid)
			{
				client.Out.SendMessage("Please enter a valid e-mail address.",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				return;
			}


			try
			{
				var obj = (GamePlayer) client.Player;

				if (obj != null)
				{
					string oldEmail = obj.Client.Account.Mail;
					obj.Client.Account.Mail = EmailAddy; // Set email

					GameServer.Database.SaveObject(obj.Client.Account); // Save account.

					// log change
					AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountEmailChange, oldEmail, EmailAddy);

					client.Out.SendMessage("Contact e-mail address set to " +
					                       obj.Client.Account.Mail + ". Thanks!",
					                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			catch (Exception)
			{
				client.Out.SendMessage("Error - Usage: /email <address>",
				                       eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		#endregion
	}

	public class EmailSyntaxValidator
	{
		private string account, domain, inputemail;
		private bool syntaxvalid = false;

		/// <summary>
		/// Initializes a new instance of the EmailSyntaxValidator
		/// </summary>
		/// <param name="email">the email to test</param>
		/// <param name="TLDrequired">indicates whether or not the 
		/// email must end with a known TLD to be considered valid</param>
		/// <remarks>
		/// The initializer creates an instance of the EmailSyntaxValidator
		/// class to validate a single email. You can specify whether or not
		/// the TLD is required and should be validated.
		/// </remarks>
		public EmailSyntaxValidator(string email, bool TLDrequired)
		{
			string tmpmail;

			//save email as validated
			inputemail = email;

			//remove <brackets> if found
			tmpmail = RemoveBrackets(email);

			//then trim
			tmpmail = Trim(tmpmail);

			//separate account from domain, quit if unable to separate
			if (!CrackEmail(tmpmail))
			{
				return;
			}

			//validate the domain, quit if domain is bad
			if (!DomainValid(TLDrequired))
			{
				return;
			}

			//if the TLD is required, validate the domain extension,
			//quit if the domain extension is bad
			if (TLDrequired && !DomainExtensionValid())
			{
				return;
			}

			//email syntax is valid
			syntaxvalid = true;
		}

		/// <summary>
		/// Gets a value indicating whether or not the email address 
		/// has valid syntax
		/// </summary>
		/// <remarks>
		/// This property returns a boolean indicating whether or not
		/// the email address has valid syntax as determined by the
		/// class.
		/// </remarks>
		/// <value>boolean indicating the validity of the email</value>
		public bool IsValid
		{
			get { return syntaxvalid; }
		}

		/// <summary>
		/// Get the domain part of the email address.
		/// </summary>
		/// <remarks>
		/// This property returns the domain part of the email
		/// address if and only if the email is considered valid
		/// by the class. Otherwise null is returned.
		/// </remarks>
		/// <value>string representing the domain of the email</value>
		public string Domain
		{
			get { return domain; }
		}

		/// <summary>
		/// Get the account part of the email address.
		/// </summary>
		/// <remarks>
		/// This property returns the account part of the email
		/// address if and only if the email is considered valid
		/// by the class. Otherwise null is returned.
		/// </remarks>
		/// <value>string representing the account of the email</value>
		public string Account
		{
			get { return account; }
		}

		/// <summary>
		/// Gets the email address as entered.
		/// </summary>
		/// <remarks>
		/// This property is filled regardless of the validity of the email.
		/// It contains the email as it was entered into the class.
		/// </remarks>
		/// <value>string representing the email address as entered</value>
		public string Address
		{
			get { return inputemail; }
		}

		/// <summary>
		/// Determines if an email has valid syntax
		/// </summary>
		/// <param name="email">the email to test</param>
		/// <param name="TLDrequired">indicates whether or not the 
		/// email must end with a known TLD to be considered valid</param>
		/// <returns>boolean indicating if the email has valid syntax</returns>
		/// <remarks>
		/// Validates an email address specifying whether or not
		/// the email is required to have a TLD that is valid.
		/// </remarks>
		public static bool Valid(string email, bool TLDrequired)
		{
			EmailSyntaxValidator v;
			bool valid;

			//call syntax validator
			v = new EmailSyntaxValidator(email, TLDrequired);

			//determine validity
			valid = v.IsValid;

			//cleanup
			v = null;

			//return indication of validity
			return valid;
		}

		/// <summary>
		/// separates email account from domain
		/// </summary>
		/// <param name="email">the email to parse</param>
		/// <returns>boolean indicating success of separation</returns>
		private bool CrackEmail(string email)
		{
			Regex re;
			Match m;
			bool ok = false;

			try
			{
				//pattern to separate email into account and domain.
				//note that we parse from right to left, thereby forcing
				//this pattern to match the last @ symbol in the email.
				re = new Regex(
					"^(.+?)\\@(.+?)$",
					RegexOptions.Singleline | RegexOptions.RightToLeft
					);

				//determine if email matches pattern (email contains one
				//or more @'s)
				if (re.IsMatch(email))
				{
					//if matched, separate account and domain as noted
					//in our pattern with the () sections.
					m = re.Match(email);

					//first group is the account
					account = m.Groups[1].Value;

					//second group is the domain
					domain = m.Groups[2].Value;

					//cleanup
					m = null;

					//indicate success
					ok = true;
				}

				//cleanup
				re = null;
			}
			catch
			{
				//catch any exceptions and just consider the string
				//unparsable
				ok = false;
			}

			//return the indication of parse success or failure
			return ok;
		}

		/// <summary>
		/// removes outer brackets from an email address
		/// </summary>
		/// <param name="input">the email to parse</param>
		/// <returns>the email without brackets</returns>
		private string RemoveBrackets(string input)
		{
			string output = null;
			Regex re;

			//pattern to match brackets or no brackets
			re = new Regex(
				"^\\<*|\\>*$",
				RegexOptions.IgnoreCase | RegexOptions.Singleline
				);

			//if email matches (it will always match this pattern)
			if (re.IsMatch(input))
			{
				//replace them with nothing
				output = re.Replace(input, "");
			}

			//cleanup
			re = null;

			//return the email without brackets
			return output;
		}

		/// <summary>
		/// trims any leading and trailing white space from the email
		/// </summary>
		/// <param name="input">the email to parse</param>
		/// <returns>the email with no leading or trailing white space</returns>
		private string Trim(string input)
		{
			string output = null;
			Regex re;

			//pattern to trim leading and trailing white space from string
			re = new Regex(
				"^\\s*|\\s*$",
				RegexOptions.IgnoreCase | RegexOptions.Singleline
				);

			//if matches
			if (re.IsMatch(input))
			{
				//remove whitespace
				output = re.Replace(input, "");
			}

			//cleanup
			re = null;

			//return the email with no leading or trailing white space
			return output;
		}

		private bool DomainValid(bool TLDrequired)
		{
			bool valid;
			Regex re;
			string pattern, emaildomain;

			if (TLDrequired)
			{
				//if the TLD is required, the pattern contains
				//a basic TLD length check at the end
				pattern = "^((([a-z0-9-]+)\\.)+)[a-z]{2,6}$";
				emaildomain = domain;
			}
			else
			{
				//when the tld is not required, the pattern is
				//the same except for the tld length check.
				//note the pattern ends with a . in the loop.
				//This means that we must append a . to the domain
				//temporarily to test the email.
				pattern = "^((([a-z0-9-]+)\\.)+)$";
				emaildomain = domain + ".";
			}

			re = new Regex(
				pattern,
				RegexOptions.IgnoreCase | RegexOptions.Singleline
				);

			//if the email matches, it's valid
			valid = re.IsMatch(emaildomain);

			//cleanup
			re = null;

			//return indication of validity
			return valid;
		}

		private bool DomainExtensionValid()
		{
			bool valid;
			Regex re;
			string domainvalidatorpattern = "";

			//pattern to validate all known TLD's
			domainvalidatorpattern += "\\.(";
			domainvalidatorpattern += "a[c-gil-oq-uwz]|"; //ac,ad,ae,af,ag,ai,al,am,an,ao,aq,ar,as,at,au,aw,az
			domainvalidatorpattern += "b[a-bd-jm-or-tvwyz]|"; //ba,bb,bd,be,bf,bg,bh,bi,bj,bm,bn,bo,br,bs,bt,bv,bw,by,bz
			domainvalidatorpattern += "c[acdf-ik-orsuvx-z]|"; //ca,cc,cd,cf,cg,ch,ci,ck,cl,cm,cn,co,cr,cs,cu,cv,cz,cy,cz
			domainvalidatorpattern += "d[ejkmoz]|"; //de,dj,dk,dm,do,dz
			domainvalidatorpattern += "e[ceghr-u]|"; //ec,ee,eg,eh,er,es,et,eu
			domainvalidatorpattern += "f[i-kmorx]|"; //fi,fj,fk,fm,fo,fr,fx
			domainvalidatorpattern += "g[abd-ilmnp-uwy]|"; //ga,gb,gd,ge,gf,gg,gh,gi,gl,gm,gn,gp,gq,gr,gs,gt,gu,gw,gy
			domainvalidatorpattern += "h[kmnrtu]|"; //hk,hm,hn,hr,ht,hu
			domainvalidatorpattern += "i[delm-oq-t]|"; //id,ie,il,im,in,io,iq,ir,is,it
			domainvalidatorpattern += "j[emop]|"; //je,jm,jo,jp
			domainvalidatorpattern += "k[eg-imnprwyz]|"; //ke,kg,kh,ki,km,kn,kp,kr,kw,ky,kz
			domainvalidatorpattern += "l[a-cikr-vy]|"; //la,lb,lc,li,lk,lr,ls,lt,lu,lv,ly
			domainvalidatorpattern += "m[acdghk-z]|"; //ma,mc,md,mg,mh,mk,ml,mm,mn,mo,mp,mq,mr,ms,mt,mu,mv,mw,mx,my,mz
			domainvalidatorpattern += "n[ace-giloprtuz]|"; //na,nc,ne,nf,ng,ni,nl,no,np,nr,nt,nu,nz
			domainvalidatorpattern += "om|"; //om
			domainvalidatorpattern += "p[ae-hk-nrtwy]|"; //pa,pe,pf,pg,ph,pk,pl,pm,pn,pr,pt,pw,py
			domainvalidatorpattern += "qa|"; //qa
			domainvalidatorpattern += "r[eouw]|"; //re,ro,ru,rw
			domainvalidatorpattern += "s[a-eg-ort-vyz]|"; //sa,sb,sc,sd,se,sg,sh,si,sj,sk,sl,sm,sn,so,sr,st,su,sv,sy,sz
			domainvalidatorpattern += "t[cdf-hjkm-prtvwz]|"; //tc,td,tf,tg,th,tj,tk,tm,tn,to,tp,tr,tt,tv,tx,tz
			domainvalidatorpattern += "u[agkmsyz]|"; //ua,ug,uk,um,us,uy,uz
			domainvalidatorpattern += "v[aceginu]|"; //va,vc,ve,vg,vy,vn,vu
			domainvalidatorpattern += "w[fs]|"; //wf,ws
			domainvalidatorpattern += "y[etu]|"; //ye,yt,yu
			domainvalidatorpattern += "z[admrw]|"; //za,zd,zm,zr,zw
			domainvalidatorpattern += "com|"; //com
			domainvalidatorpattern += "edu|"; //edu
			domainvalidatorpattern += "net|"; //net
			domainvalidatorpattern += "org|"; //org
			domainvalidatorpattern += "mil|"; //mil
			domainvalidatorpattern += "gov|"; //gov
			domainvalidatorpattern += "biz|"; //biz
			domainvalidatorpattern += "pro|"; //pro
			domainvalidatorpattern += "aero|"; //aero
			domainvalidatorpattern += "coop|"; //coop
			domainvalidatorpattern += "info|"; //info
			domainvalidatorpattern += "name|"; //name
			domainvalidatorpattern += "int|"; //int
			domainvalidatorpattern += "museum"; //museum
			domainvalidatorpattern += ")$";

			re = new Regex(
				domainvalidatorpattern,
				RegexOptions.IgnoreCase | RegexOptions.Singleline
				);

			//if domain matches pattern, it has a valid TLD
			valid = re.IsMatch(domain);

			//cleanup
			re = null;

			//return an indication of TLD validity
			return valid;
		}
	}
}