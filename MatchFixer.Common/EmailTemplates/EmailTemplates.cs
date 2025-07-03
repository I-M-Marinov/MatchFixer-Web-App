using System.Text.Encodings.Web;

namespace MatchFixer.Common.EmailTemplates
{
	public static class EmailTemplates
	{

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
								<img src='{logoUrl}' alt='MatchFixer Logo' style='height: 80px; margin-bottom: 10px;' />
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
								   All Rights Reserved. MatchFixer ® 2025
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
						<img src='{logoUrl}' alt='MatchFixer Logo' style='height: 80px; margin-bottom: 10px;' />
					</div>
					<div style='padding: 30px; text-align: center;'>
						<h2 style='color: #333;'>Your password was changed on {userTimeFormatted}</h2>
						<p style='margin-top: 30px; font-size: 13px; color: #e32b17;'>
							If you did not reset your password, please review your account and ensure it is not compromised.
						</p>
						<p style='margin-top: 15px; font-size: 12px; color: #040bcf;'>
							All Rights Reserved. MatchFixer ® 2025
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
								<img src='{logoUrl}' alt='MatchFixer Logo' style='height: 80px; margin-bottom: 10px;' />
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
								   All Rights Reserved. MatchFixer ® 2025
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
				            <img src='{logoUrl}' alt='MatchFixer Logo' style='height: 80px; margin-bottom: 10px;' />
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
				               All Rights Reserved. MatchFixer ® 2025
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
					            <img src='{logoUrl}' alt='MatchFixer Logo' style='height: 80px; margin-bottom: 10px;' />
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
					               All Rights Reserved. MatchFixer ® 2025
					            </p>
					        </div>
					    </div>
					</body>
				</html>";
		}

	}

}
