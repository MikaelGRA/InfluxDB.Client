using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;
using Xunit;

namespace Vibrant.InfluxDB.Client.Tests
{
   [Collection( "InfluxClient collection" )]
   public class AuthenticationTests
   {
      private InfluxClient _client;

      public AuthenticationTests( InfluxClientFixture fixture )
      {
         _client = fixture.Client;
      }

      [Fact]
      public async Task Should_Create_Show_And_Delete_Admin_User()
      {
         await _client.CreateAdminUserAsync( "at1User", "somePassword" );

         var result = await _client.ShowUsersAsync();
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.Username == "at1User" && x.IsAdmin );

         await _client.DropUserAsync( "at1User" );

         result = await _client.ShowUsersAsync();
         Assert.Equal( 1, result.Series.Count );

         series = result.Series[ 0 ];
         Assert.DoesNotContain( series.Rows, x => x.Username == "at1User" );
      }

      [Fact]
      public async Task Should_Create_Show_And_Delete_User()
      {
         await _client.CreateUserAsync( "at2User", "somePassword" );

         var result = await _client.ShowUsersAsync();
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.Username == "at2User" && !x.IsAdmin );

         await _client.DropUserAsync( "at2User" );

         result = await _client.ShowUsersAsync();
         Assert.Equal( 1, result.Series.Count );

         series = result.Series[ 0 ];
         Assert.DoesNotContain( series.Rows, x => x.Username == "at2User" );
      }

      [Fact]
      public async Task Should_Grant_And_Revoke_Admin_Privileges_To_User()
      {
         await _client.CreateUserAsync( "at3User", "somePassword" );

         var result = await _client.ShowUsersAsync();
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.Username == "at3User" && !x.IsAdmin );

         await _client.GrantAdminPrivilegesAsync( "at3User" );

         result = await _client.ShowUsersAsync();
         Assert.Equal( 1, result.Series.Count );

         series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.Username == "at3User" && x.IsAdmin );

         await _client.RevokeAdminPrivilegesAsync( "at3User" );

         result = await _client.ShowUsersAsync();
         Assert.Equal( 1, result.Series.Count );

         series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.Username == "at3User" && !x.IsAdmin );

         await _client.DropUserAsync( "at3User" );

         result = await _client.ShowUsersAsync();
         Assert.Equal( 1, result.Series.Count );

         series = result.Series[ 0 ];
         Assert.DoesNotContain( series.Rows, x => x.Username == "at3User" );
      }

      [Fact]
      public async Task Should_Grant_And_Revoke_Privileges_To_User()
      {
         await _client.CreateUserAsync( "at4User", "somePassword" );

         await _client.GrantPrivilegeAsync( InfluxClientFixture.DatabaseName, DatabasePriviledge.Read, "at4User" );

         var result = await _client.ShowGrantsAsync( "at4User" );
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.Database == InfluxClientFixture.DatabaseName && x.Privilege == DatabasePriviledge.Read );

         await _client.RevokePrivilegeAsync( InfluxClientFixture.DatabaseName, DatabasePriviledge.Read, "at4User" );

         result = await _client.ShowGrantsAsync( "at4User" );
         Assert.Equal( 1, result.Series.Count );

         series = result.Series[ 0 ];
         Assert.DoesNotContain( series.Rows, x => x.Database == InfluxClientFixture.DatabaseName && x.Privilege == DatabasePriviledge.Read );

         await _client.DropUserAsync( "at4User" );
      }

      [Fact]
      public async Task Should_Login_As_User_And_Set_Password()
      {
         await _client.CreateUserAsync( "at5User", "somePassword" );

         await _client.SetPasswordAsync( "at5User", "otherPassword" );

         var at5UserClient = new InfluxClient( new Uri( "http://localhost:8083" ), "at5User", "otherPassword" );
         
         // TODO: Some operations
         
         await _client.DropUserAsync( "at5User" );
      }
   }
}
