using System.Text.Encodings.Web;
using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;

namespace MatchFixer.Common.EmailTemplates
{
	public static class EmailTemplates
	{
		private static readonly Random _random = new();

		public const string SubjectPleaseConfirmEmail = "MatchFixer - Please confirm your email"; // Welcome email & Resend email confirmation
		public const string SubjectEmailAddressChanged = "MatchFixer - Your email address was changed"; // Email subject for the email changed emails
		public const string SubjectAccountPasswordChanged = "MatchFixer - Your account password was changed"; // Email subject for the password changed emails
		public const string SubjectPasswordResetRequested = "MatchFixer - Password reset requested"; // Email subject for the password reset requested emails
		public const string SubjectHappyBirthdayFromMatchFixer = "🎂 Happy Birthday from MatchFixer!"; // Email subject for the birthday emails

		public const string SubjectBetSlipWon    = "🎉 You won! Your MatchFixer bet slip has been settled";
		public const string SubjectBetSlipVoided = "↩️ Your MatchFixer bet slip has been voided and refunded";

		public record BetRowEmailData(string HomeTeam, string AwayTeam, string Pick, string Status);

		public static string SubjectTrophyWonEmail(string trophyName)
		{
			return $@"🏆 You’ve won the '{trophyName}' Trophy!"; 
		}
		public static string WelcomeEmail(string logoUrl, string callbackUrl)
		{
			return $@"
				<!DOCTYPE html>
				<html>
					<head>
						<meta charset='UTF-8'>
						<title>Confirm Your Email</title>
					</head>
					<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>
						<div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
							<div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
								<div style='max-width: 320px; margin: 0 auto;'>
									<img src='{logoUrl}' alt='MatchFixer Logo' width='320' style='width: 100%; height: auto; display: block;' />
								</div>
							</div>
							<div style='padding: 30px; text-align: center;'>
								<h2 style='color: #333;'>🎉 Welcome to MatchFixer (Don’t worry, it’s legal... we checked)</h2>
								<p style='color: #555; font-size: 16px;'>
									Hey there, future match “fixer”! 😉<br />
									You're now part of the most suspiciously fun soccer betting platform on the planet.<br />
									(Just kidding. We keep it clean... mostly.)<br />
									Here at MatchFixer, you're not bribing refs — you're calling the shots with your football smarts.<br />
									From miracle goals to VAR-induced chaos, your bets could make legends (or memes).<br />
									So lace up those virtual boots.<br />
									It’s time to outwit, outbet, and maybe out-laugh the competition.<br /><br />
									Let's get you started on your journey!
								</p>
								<p style='color: #bf2217; font-size: 12px;'>
									**Please note that this account will be active for 30 minutes and will be removed if not confirmed in time.
								</p>
								<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
								   style='display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #27ae60; border: 2px black solid; text-decoration: none; color: black; border-radius: 6px; font-weight: bold;'>
									Confirm Your Email
								</a>
								<p style='margin-top: 30px; font-size: 13px; color: #888;'>
									If you did not sign up for MatchFixer, please ignore this email.
								</p>
								<p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
								   All Rights Reserved. MatchFixer ® 2025 - {DateTime.UtcNow.Year}
								</p>
							</div>
						</div>
					</body>
				</html>";
		}
		public static string PasswordChanged(string logoUrl, string userTimeFormatted)
		{
			return $@"
			<!DOCTYPE html>
			<html>
			<head>
				<meta charset='UTF-8'>
				<title>Password Changed</title>
			</head>
			<body style='font-family: Helvetica, sans-serif; background-color: #f4f4f4; padding: 30px;'>
				<div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
					<div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
						<div style='max-width: 320px; margin: 0 auto;'>
									<img src='{logoUrl}' alt='MatchFixer Logo' width='320' style='width: 100%; height: auto; display: block;' />
								</div>
					</div>
					<div style='padding: 30px; text-align: center;'>
						<h2 style='color: #333;'>Your password was changed on {userTimeFormatted}</h2>
						<p style='margin-top: 30px; font-size: 13px; color: #e32b17;'>
							If you did not reset your password, please review your account and ensure it is not compromised.
						</p>
						<p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
							All Rights Reserved. MatchFixer ® 2025 - {DateTime.UtcNow.Year}
						</p>
					</div>
				</div>
			</body>
			</html>";
		}
		public static string EmailConfirmation(string logoUrl, string callbackUrl)
		{
			return $@"
				<!DOCTYPE html>
				<html>
					<head>
						<meta charset='UTF-8'>
						<title>Confirm your email</title>
					</head>
					<body style='font-family: Helvetica, sans-serif; background-color: #f4f4f4; padding: 30px;'>
						<div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
							<div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
								<div style='max-width: 320px; margin: 0 auto;'>
									<img src='{logoUrl}' alt='MatchFixer Logo' width='320' style='width: 100%; height: auto; display: block;' />
								</div>
							</div>
							<div style='padding: 30px; text-align: center;'>
								<h2 style='color: #333;'>Please follow the link below to confirm your MatchFixer Account</h2>

								<a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
								   style='display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #27ae60; border: 2px black solid; color: black; text-decoration: none; border-radius: 6px; font-weight: bold;'>
									Confirm Your Email
								</a>

								<p style='margin-top: 30px; font-size: 13px; color: #888;'>
									If you did not sign up for MatchFixer, please ignore this email.
								</p>
								<p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
								   All Rights Reserved. MatchFixer ® 2025 - {DateTime.UtcNow.Year}
								</p>
							</div>
						</div>
					</body>
				</html>";
		}
		public static string PasswordResetEmail(string logoUrl, string callbackUrl)
		{
			return $@"
				<!DOCTYPE html>
				<html>
				<head>
				    <meta charset='UTF-8'>
				    <title>Reset Your Password</title>
				</head>
				<body style='font-family: Helvetica, sans-serif; background-color: #f4f4f4; padding: 30px;'>
				    <div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
				        <div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
				            <div style='max-width: 320px; margin: 0 auto;'>
									<img src='{logoUrl}' alt='MatchFixer Logo' width='320' style='width: 100%; height: auto; display: block;' />
								</div>
				        </div>
				        <div style='padding: 30px; text-align: center;'>
				            <h2 style='color: #333;'>Please follow the link below to reset your MatchFixer password</h2>
				            
				            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
				               style='display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #4287f5; border: 2px #05f09a solid; color: black; text-decoration: none; border-radius: 6px; font-weight: bold;'>
				                Reset Your Password
				            </a>

				            <p style='margin-top: 30px; font-size: 13px; color: #888;'>
				                If you did not request this password reset, please review your account and ensure it is not compromised.
				            </p>
							<p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
				               All Rights Reserved. MatchFixer ® 2025 - {DateTime.UtcNow.Year}
				            </p>
				        </div>
				    </div>
				</body>
				</html>";
		} 
		public static string EmailUpdateConfirmation(string logoUrl, string confirmationUrl)
		{
			return $@"
				<!DOCTYPE html>
				<html>
					<head>
					    <meta charset='UTF-8'>
					    <title>Confirm your email</title>
					</head>
					<body style='font-family: Helvetica, sans-serif; background-color: #f4f4f4; padding: 30px;'>
					    <div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
					        <div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
					            <div style='max-width: 320px; margin: 0 auto;'>
									<img src='{logoUrl}' alt='MatchFixer Logo' width='320' style='width: 100%; height: auto; display: block;' />
								</div>
					        </div>
					        <div style='padding: 30px; text-align: center;'>
					            <h2 style='color: #333;'>Please follow the link below to confirm your new Email Address</h2>
					            <a href='{HtmlEncoder.Default.Encode(confirmationUrl)}' 
					               style='display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #27ae60; border: 2px black solid; color: black; text-decoration: none; border-radius: 6px; font-weight: bold;'>
					                Confirm Your Email
					            </a>
					            <p style='margin-top: 30px; font-size: 13px; color: #888;'>
					                If you did not request this email change, your account might be compromised.
					            </p>
					            <p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
					               All Rights Reserved. MatchFixer ® 2025 - {DateTime.UtcNow.Year}
					            </p>
					        </div>
					    </div>
					</body>
				</html>";
		}
		public static string BirthdayEmail(string logoUrl, string username)
		{
			return $@"
		<!DOCTYPE html>
		<html>
		<head>
			<meta charset='UTF-8'>
			<title>Happy Birthday!</title>
		</head>
		<body style='font-family: Helvetica, sans-serif; background-color: #f4f4f4; padding: 30px;'>
			<div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
				<div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
					<div style='max-width: 320px; margin: 0 auto;'>
									<img src='{logoUrl}' alt='MatchFixer Logo' width='320' style='width: 100%; height: auto; display: block;' />
								</div>
				</div>
				<div style='padding: 30px; text-align: center;'>
					<h2 style='color: #333;'>🎉 Happy Birthday, {username}! 🎂</h2>
					<p style='color: #555; font-size: 16px;'>
						Everyone at MatchFixer wishes you a fantastic day filled with joy, luck, and a few winning bets. 🍀<br/><br/>
						To celebrate, we’ve rigged the algorithm in your favor today… just kidding 😅 (or are we?).<br/>
						Thanks for being part of the most dangerously fun betting community on the internet.
					</p>
					<p style='font-size: 15px; color: #0dab76; font-weight: bold; margin-top: 20px;'>
						🎁 Enjoy your day and make it count!
					</p>

					<p style='font-size: 15px; color: #0dab76; font-weight: bold; margin-top: 20px;'>
						To make it more special for you we have credited your wallet €10 for you to bet on anything you want ! Best of luck !
					</p>

					<p style='margin-top: 30px; font-size: 13px; color: #888;'>
						If you received this by mistake, it’s probably because your doppelganger shares your birthday.
					</p>
					<p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
						All Rights Reserved. MatchFixer ® 2025 - {DateTime.UtcNow.Year}
					</p>
				</div>
			</div>
		</body>
		</html>";
		}
		public static string EmailTrophyWon(string logoUrl, string trophyImageUrl, string trophyName, string profileUrl)
		{
			return $@"
			    <!DOCTYPE html>
					<html>
					    <head>
					        <meta charset='UTF-8'>
					        <title>You've Won a Trophy!</title>
					    </head>
					    <body style='font-family: Helvetica, sans-serif; background-color: #f4f4f4; padding: 30px;'>
					        <div style='max-width: 1000px; margin: auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
					            <div style='text-align: center; background-color: #2c3e50; padding: 20px 0;'>
					                <div style='max-width: 320px; margin: 0 auto;'>
									<img src='{logoUrl}' alt='MatchFixer Logo' width='320' style='width: 100%; height: auto; display: block;' />
								</div>
					            </div>
					            <div style='padding: 30px; text-align: center;'>
					                <h2 style='color: #333;'>🏆 Congratulations!</h2>
					                <p style='font-size: 1.1em; color: #555;'>You have just unlocked a new trophy:</p>
					                <h3 style='color: #27ae60; margin-top: 10px;'>{trophyName}</h3>
					                <table role='presentation' width='100%'>
									    <tr>
									        <td align='center'>
									            <img src='{trophyImageUrl}' alt='{trophyName}' style='max-width: 200px;' />
									        </td>
									    </tr>
									</table>
					                
					                <a href='{HtmlEncoder.Default.Encode(profileUrl)}' 
					                   style='display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #27ae60; border: 2px black solid; color: black; text-decoration: none; border-radius: 6px; font-weight: bold;'>
					                    View It In Your Trophy Cabinet
					                </a>

					                <p style='margin-top: 30px; font-size: 13px; color: #888;'>
					                    Keep betting smart to earn even more trophies!
					                </p>
					                <p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
					                   All Rights Reserved. MatchFixer ® 2025 - {DateTime.UtcNow.Year}
					                </p>
					            </div>
					        </div>
					    </body>
					</html>";
		}
		public static string BlastTemplate(string subject, string contentHtml)
		{
			return $@"
				<!DOCTYPE html>
				<html lang='en'>
				  <head>
				    <meta charset='UTF-8'>
				    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
				    <title>{HtmlEncoder.Default.Encode(subject)}</title>
				  </head>
				  <body style='margin:0;padding:0;background-color:#070d1a;font-family:""Helvetica Neue"",Helvetica,Arial,sans-serif;'>
				    <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#070d1a;padding:40px 20px;'>
				      <tr>
				        <td align='center'>
				          <table width='600' cellpadding='0' cellspacing='0' style='max-width:600px;width:100%;border-radius:16px;overflow:hidden;box-shadow:0 0 60px rgba(13,171,118,0.12),0 24px 60px rgba(0,0,0,0.6);'>

				            <!-- HEADER -->
				            <tr>
				              <td style='background:linear-gradient(135deg,#0a1628 0%,#162040 100%);padding:30px 40px;text-align:center;border-bottom:1px solid rgba(13,171,118,0.25);'>
				                <img src='{LogoUrl}' alt='MatchFixer' width='260' style='width:260px;max-width:100%;height:auto;display:block;margin:0 auto;' />
				              </td>
				            </tr>

				            <!-- SUBJECT BANNER -->
				            <tr>
				              <td style='background:linear-gradient(90deg,#0dab76 0%,#0a8f62 100%);padding:13px 40px;text-align:center;'>
				                <p style='margin:0;font-size:11px;font-weight:700;color:#fff;letter-spacing:2.5px;text-transform:uppercase;'>{HtmlEncoder.Default.Encode(subject)}</p>
				              </td>
				            </tr>

				            <!-- CONTENT -->
				            <tr>
				              <td style='background-color:#0c1525;padding:40px 40px 32px;'>
				                <div style='color:#cbd5e1;font-size:15px;line-height:1.8;'>
				                  {contentHtml}
				                </div>
				              </td>
				            </tr>

				            <!-- FOOTER -->
				            <tr>
				              <td style='background-color:#070d1a;padding:20px 40px;text-align:center;border-top:1px solid rgba(255,255,255,0.05);'>
				                <p style='margin:0;font-size:12px;color:#3d5068;letter-spacing:0.3px;'>All Rights Reserved. MatchFixer ® 2025 – {DateTime.UtcNow.Year}</p>
				              </td>
				            </tr>

				          </table>
				        </td>
				      </tr>
				    </table>
				  </body>
				</html>";
		}
		public static string MatchAddedEmail(string logoUrl, string homeTeam, string awayTeam, string homeLogo, string awayLogo, string matchTime, string link)
		{
			var homeLogoHtml = !string.IsNullOrEmpty(homeLogo)
				? $"<img src='{homeLogo}' style='width:50px;max-width:100%;height:auto;display:block;margin:0 auto 6px auto;' />"
				: "";

			var awayLogoHtml = !string.IsNullOrEmpty(awayLogo)
				? $"<img src='{awayLogo}' style='width:50px;max-width:100%;height:auto;display:block;margin:0 auto 6px auto;' />"
				: "";

			return $@"
						<!DOCTYPE html>
						<html>
						<body style='font-family: Arial, sans-serif; background:#f4f6f8; padding:20px;'>

						<div style='max-width:1000px;margin:auto;background:white;border-radius:12px;overflow:hidden;
						            box-shadow:0 10px 30px rgba(0,0,0,0.1);'>

						    <!-- HEADER -->
						    <div style='background:#2c3e50;padding:20px;text-align:center;'>
						        <div style='max-width:320px;margin:0 auto;'><img src='{logoUrl}' alt='MatchFixer Logo' width='320' style='width:100%;height:auto;display:block;' /></div>
						    </div>

						    <!-- CONTENT -->
						    <div style='padding:30px;text-align:center;'>

						        <h2 style='margin-bottom:10px;'>⚽ Your Favorite Team is Playing!</h2>

						        <p style='color:#666;font-size:15px;margin-bottom:25px;'>
						            Looks like something interesting just got added… 👀
						        </p>

						        <!-- MATCH ROW -->
						        <table width='100%' cellpadding='0' cellspacing='0' style='margin:20px 0; text-align:center;'>
						            <tr>

						                <!-- HOME -->
						                <td style='width:40%; text-align:center;'>
						                    {homeLogoHtml}
						                    <span style='font-size:16px;font-weight:600;color:#2c3e50;'>{homeTeam}</span>
						                </td>

						                <!-- VS -->
						                <td style='width:20%; text-align:center; font-weight:bold; color:#666;'>
						                    VS
						                </td>

						                <!-- AWAY -->
						                <td style='width:40%; text-align:center;'>
						                    {awayLogoHtml}
						                    <span style='font-size:16px;font-weight:600;color:#2c3e50;'>{awayTeam}</span>
						                </td>

						            </tr>
						        </table>

						        <!-- TIME -->
						        <p style='font-size:15px;color:#555;margin-top:15px;'>
						            🕒 Kickoff: <strong>{matchTime}</strong>
						        </p>

						        <!-- CTA -->
						        <a href='{link}'
						           style='display:inline-block;margin-top:25px;padding:12px 28px;
						                  background:linear-gradient(135deg,#27ae60,#2ecc71);
						                  color:black;border-radius:8px;text-decoration:none;
						                  font-weight:bold;font-size:15px;
						                  box-shadow:0 6px 15px rgba(39,174,96,0.4);'>
						            Place Your Bet 🎯
						        </a>

						        <!-- EXTRA -->
						        <p style='margin-top:30px;font-size:13px;color:#777;'>
						            We’re not saying it’s fixed… but we’re not NOT saying it either 😏
						        </p>

						    </div>

						    <!-- FOOTER -->
						    <div style='background:#f1f1f1;padding:15px;text-align:center;font-size:12px;color:#777;'>
						        MatchFixer © 2025-{DateTime.UtcNow.Year} — Your personal space for fixing bets
						    </div>

						</div>

						</body>
						</html>";
		}


		public static string BetSlipSettledEmail(
			string logoUrl,
			string userName,
			string outcome,
			decimal stake,
			decimal totalOdds,
			decimal? winAmount,
			IEnumerable<BetRowEmailData> bets)
		{
			var outcomeColor = outcome switch
			{
				"Won"    => "#27ae60",
				"Lost"   => "#e74c3c",
				"Voided" => "#95a5a6",
				_        => "#555"
			};

			var outcomeEmoji = outcome switch
			{
				"Won"    => "🎉",
				"Lost"   => "😞",
				"Voided" => "↩️",
				_        => "📋"
			};

			var payoutRow = outcome switch
			{
				"Won"    => $"<tr><td style='padding:6px 12px;color:#555;'>Payout</td><td style='padding:6px 12px;font-weight:bold;color:#27ae60;'>€{winAmount:F2}</td></tr>",
				"Voided" => $"<tr><td style='padding:6px 12px;color:#555;'>Refunded</td><td style='padding:6px 12px;font-weight:bold;color:#95a5a6;'>€{stake:F2}</td></tr>",
				_        => ""
			};

			var betRowsHtml = string.Join("", bets.Select(b =>
			{
				var statusColor = b.Status switch
				{
					"Won"    => "#27ae60",
					"Lost"   => "#e74c3c",
					"Voided" => "#95a5a6",
					_        => "#f39c12"
				};

				var statusBadge = $"<span style='background:{statusColor};color:white;padding:2px 8px;border-radius:4px;font-size:12px;font-weight:bold;'>{b.Status.ToUpper()}</span>";

				return $@"
					<tr style='border-bottom:1px solid #eee;'>
						<td style='padding:10px 12px;color:#333;'>{b.HomeTeam} <span style='color:#aaa;'>vs</span> {b.AwayTeam}</td>
						<td style='padding:10px 12px;color:#555;text-align:center;'>{b.Pick}</td>
						<td style='padding:10px 12px;text-align:center;'>{statusBadge}</td>
					</tr>";
			}));

			return $@"
				<!DOCTYPE html>
				<html>
				<head><meta charset='UTF-8'><title>Bet Slip Result</title></head>
				<body style='font-family:Helvetica,sans-serif;background-color:#f4f4f4;padding:30px;'>
					<div style='max-width:1000px;margin:auto;background:white;border-radius:8px;overflow:hidden;box-shadow:0 4px 12px rgba(0,0,0,0.1);'>

						<div style='text-align:center;background-color:#2c3e50;padding:20px 0;'>
							<div style='max-width:320px;margin:0 auto;'><img src='{logoUrl}' alt='MatchFixer Logo' width='320' style='width:100%;height:auto;display:block;' /></div>
						</div>

						<div style='padding:30px;text-align:center;'>
							<h2 style='color:{outcomeColor};margin-bottom:4px;'>{outcomeEmoji} Bet Slip {outcome}</h2>
							<p style='color:#777;margin-top:0;'>Hi {userName}, here is the result of your bet slip.</p>

							<table style='margin:20px auto;border-collapse:collapse;font-size:15px;'>
								<tr>
									<td style='padding:6px 12px;color:#555;'>Stake</td>
									<td style='padding:6px 12px;font-weight:bold;color:#333;'>€{stake:F2}</td>
								</tr>
								<tr>
									<td style='padding:6px 12px;color:#555;'>Total Odds</td>
									<td style='padding:6px 12px;font-weight:bold;color:#333;'>{totalOdds:F2}x</td>
								</tr>
								{payoutRow}
							</table>

							<h4 style='color:#333;margin-bottom:8px;text-align:left;'>Selections</h4>
							<table style='width:100%;border-collapse:collapse;font-size:14px;text-align:left;'>
								<thead>
									<tr style='background:#f8f8f8;'>
										<th style='padding:8px 12px;color:#888;font-weight:600;'>Match</th>
										<th style='padding:8px 12px;color:#888;font-weight:600;text-align:center;'>Pick</th>
										<th style='padding:8px 12px;color:#888;font-weight:600;text-align:center;'>Result</th>
									</tr>
								</thead>
								<tbody>
									{betRowsHtml}
								</tbody>
							</table>

							<p style='margin-top:30px;font-size:13px;color:#888;'>Keep betting smart — your next win could be around the corner.</p>
							<p style='margin-top:10px;font-size:12px;color:#040bcf;'>All Rights Reserved. MatchFixer ® 2025 - {DateTime.UtcNow.Year}</p>
						</div>
					</div>
				</body>
				</html>";
		}

		// ══════════════════════════════════════════════════════════════════════
		// Blast template body snippets 
		// ══════════════════════════════════════════════════════════════════════

		// Shared inline-CSS constants (email-safe, no flexbox)
		private const string _ctaBtn   = "display:inline-block;padding:15px 40px;background:linear-gradient(135deg,#0dab76,#0a8f62);color:#fff;text-decoration:none;border-radius:10px;font-weight:700;font-size:16px;letter-spacing:0.5px;box-shadow:0 6px 25px rgba(13,171,118,0.35);";
		private const string _heroTitle = "margin:14px 0 6px;font-size:26px;color:#fff;font-weight:800;line-height:1.2;";
		private const string _heroSub   = "margin:0 0 28px;font-size:15px;color:#94a3b8;";
		private const string _accentTxt = "color:#0dab76;font-weight:600;";
		private const string _footerTxt = "margin:28px 0 0;font-size:13px;color:#475569;";

		public static string BlastBodyWelcomeBack() => $@"
<table width='100%' cellpadding='0' cellspacing='0'>
  <tr><td style='text-align:center;padding:10px 0 32px;'>
    <p style='font-size:44px;margin:0 0 4px;'>👋</p>
    <h2 style='{_heroTitle}'>We Miss You</h2>
    <p style='{_heroSub}'>It&rsquo;s been a while since your last bet on <span style='{_accentTxt}'>MatchFixer</span></p>
    <p style='color:#cbd5e1;font-size:15px;line-height:1.8;margin:0 0 24px;'>
      We&rsquo;ve got exciting matches lined up and some big odds waiting to be claimed.<br/>
      Your next big win could be just one slip away.
    </p>
    <table cellpadding='0' cellspacing='0' style='margin:0 auto 28px;background:rgba(13,171,118,0.08);border:1px solid rgba(13,171,118,0.28);border-radius:12px;'>
      <tr><td style='padding:15px 28px;text-align:center;'>
        <p style='margin:0;font-size:14px;color:#0dab76;font-weight:600;'>⚽&nbsp; Today&rsquo;s matches are live &mdash; your odds are waiting</p>
      </td></tr>
    </table>
    <a href='[INSERT LINK]' style='{_ctaBtn}'>Back to the Action &rarr;</a>
    <p style='{_footerTxt}'>Good luck &mdash; The MatchFixer Team</p>
  </td></tr>
</table>";

		public static string BlastBodyWorldCup() => $@"
<table width='100%' cellpadding='0' cellspacing='0'>
  <tr><td style='text-align:center;padding:10px 0 32px;'>
    <p style='font-size:50px;margin:0 0 4px;'>🌍</p>
    <h2 style='{_heroTitle}'>FIFA World Cup 2026</h2>
    <p style='{_heroSub}'>The biggest stage in football is here</p>
    <table width='100%' cellpadding='0' cellspacing='0' style='background:linear-gradient(135deg,rgba(13,171,118,0.12),rgba(51,112,168,0.12));border:1px solid rgba(13,171,118,0.22);border-radius:14px;margin-bottom:28px;'>
      <tr><td style='padding:22px 28px;text-align:center;'>
        <p style='margin:0 0 10px;font-size:16px;color:#fff;font-weight:700;'>🏆&nbsp; Group Stage &middot; Round of 16 &middot; Quarter-Finals</p>
        <p style='margin:0;font-size:14px;color:#94a3b8;line-height:1.75;'>
          New matches drop daily. Back your favourites, call the upsets,<br/>and build your winning slip &mdash; all on <span style='{_accentTxt}'>MatchFixer</span>.
        </p>
      </td></tr>
    </table>
    <a href='[INSERT LINK]' style='{_ctaBtn}'>Bet on the World Cup &rarr;</a>
    <p style='{_footerTxt}'>Good luck &mdash; The MatchFixer Team</p>
  </td></tr>
</table>";

		public static string BlastBodyWeekend() => $@"
<table width='100%' cellpadding='0' cellspacing='0'>
  <tr><td style='text-align:center;padding:10px 0 20px;'>
    <p style='font-size:44px;margin:0 0 4px;'>⚽</p>
    <h2 style='{_heroTitle}'>Big Weekend Ahead</h2>
    <p style='{_heroSub}'>Don&rsquo;t let it pass without your bets placed</p>
  </td></tr>
  <tr><td style='padding:0 0 28px;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='border-left:4px solid #0dab76;background:rgba(255,255,255,0.04);border-radius:0 10px 10px 0;margin-bottom:10px;'>
      <tr><td style='padding:13px 18px;font-size:14px;color:#e2e8f0;'>🏟️&nbsp; Top-flight leagues with deep markets</td></tr>
    </table>
    <table width='100%' cellpadding='0' cellspacing='0' style='border-left:4px solid #3370a8;background:rgba(255,255,255,0.04);border-radius:0 10px 10px 0;margin-bottom:10px;'>
      <tr><td style='padding:13px 18px;font-size:14px;color:#e2e8f0;'>🌍&nbsp; International action across all competitions</td></tr>
    </table>
    <table width='100%' cellpadding='0' cellspacing='0' style='border-left:4px solid #0dab76;background:rgba(255,255,255,0.04);border-radius:0 10px 10px 0;'>
      <tr><td style='padding:13px 18px;font-size:14px;color:#e2e8f0;'>💰&nbsp; Build your slip and make it count this weekend</td></tr>
    </table>
  </td></tr>
  <tr><td style='text-align:center;padding-bottom:12px;'>
    <a href='[INSERT LINK]' style='{_ctaBtn}'>View Weekend Fixtures &rarr;</a>
    <p style='{_footerTxt}'>Good luck &mdash; The MatchFixer Team</p>
  </td></tr>
</table>";

		/// <param name="boosts">Sequence of (HomeTeam, AwayTeam, BoostLabel e.g. "+0.25", ExpiryLabel e.g. "12 Jul, 20:00")</param>
		public static string BlastBodyBoostedMatches(IEnumerable<(string Home, string Away, string Boost, string Until)> boosts)
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendLine($@"
<table width='100%' cellpadding='0' cellspacing='0'>
  <tr><td style='text-align:center;padding:10px 0 24px;'>
    <p style='font-size:44px;margin:0 0 4px;'>🔥</p>
    <h2 style='margin:12px 0 4px;font-size:26px;color:#fff;font-weight:800;'>Boosted Odds &mdash; Live Now</h2>
    <p style='margin:0 0 28px;font-size:15px;color:#94a3b8;'>For a limited time only. Grab them before they&rsquo;re gone.</p>
  </td></tr>
  <tr><td style='padding-bottom:28px;'>");

			foreach (var (home, away, boost, until) in boosts)
			{
				sb.AppendLine($@"
    <table width='100%' cellpadding='0' cellspacing='0' style='background:rgba(249,115,22,0.07);border:1px solid rgba(249,115,22,0.28);border-radius:12px;margin-bottom:10px;'>
      <tr>
        <td style='padding:14px 18px;'>
          <p style='margin:0;font-size:15px;font-weight:700;color:#fff;'>{home} <span style='color:#64748b;font-weight:400;'>vs</span> {away}</p>
          <p style='margin:5px 0 0;font-size:12px;color:#64748b;'>&#9200; Expires: {until} UTC</p>
        </td>
        <td style='padding:14px 18px;text-align:right;white-space:nowrap;vertical-align:middle;'>
          <table cellpadding='0' cellspacing='0' style='display:inline-table;background:linear-gradient(135deg,#f97316,#dc6011);border-radius:8px;'>
            <tr><td style='padding:8px 16px;text-align:center;'>
              <span style='display:block;font-size:15px;font-weight:800;color:#fff;line-height:1.1;'>{boost}</span>
              <span style='display:block;font-size:9px;color:rgba(255,255,255,0.75);letter-spacing:1.5px;text-transform:uppercase;margin-top:2px;'>BOOST</span>
            </td></tr>
          </table>
        </td>
      </tr>
    </table>");
			}

			sb.AppendLine($@"
  </td></tr>
  <tr><td style='text-align:center;'>
    <a href='[INSERT LINK]' style='{_ctaBtn}'>Claim Boosted Odds &rarr;</a>
    <p style='margin:28px 0 0;font-size:13px;color:#475569;'>Good luck &mdash; The MatchFixer Team</p>
  </td></tr>
</table>");

			return sb.ToString();
		}

		public static string GetMatchNotificationSubject(string favoriteTeam, string opponent)
		{
			var subjects = new List<string>
			{
				$"⚽ {favoriteTeam} has a new match — don’t miss it!",
				$"🔥 {favoriteTeam} is playing soon — place your bets!",
				$"👀 {favoriteTeam} just got scheduled… looks interesting",
				$"🎯 Your team {favoriteTeam} is back — time to fix your bets",
				$"⚽ {favoriteTeam} vs {opponent} — bet now",
				$"⚽ {favoriteTeam} is playing — don’t miss your chance!",
				$"🔥 {favoriteTeam} vs {opponent} just dropped — get in early!"
			};

			return subjects[_random.Next(subjects.Count)];
		}

	}

}
