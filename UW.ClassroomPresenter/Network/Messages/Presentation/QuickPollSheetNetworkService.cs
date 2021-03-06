using System;
using System.Collections.Generic;
using System.Text;
using UW.ClassroomPresenter.Model.Presentation;
using UW.ClassroomPresenter.Model;
using UW.ClassroomPresenter.Network.Groups;

namespace UW.ClassroomPresenter.Network.Messages.Presentation {
    /// <summary>
    /// This only exists for consistency and so that SheetNetworkService.ForSheet can return something non-null if
    /// the sheet is an ImageSheetModel.
    /// </summary>
    class QuickPollSheetNetworkService : SheetNetworkService {
        private bool m_Disposed;
        private readonly QuickPollSheetModel sheet;
        private SlideModel m_SlideModel;

        public QuickPollSheetNetworkService( SendingQueue sender, PresentationModel presentation, DeckModel deck, SlideModel slide, QuickPollSheetModel sheet, SheetMessage.SheetCollection selector )
            : base( sender, presentation, deck, slide, sheet, selector ) {
            this.sheet = sheet;
            this.m_SlideModel = slide;
        }

        protected override void HandleBoundsChanged( object sender, PropertyEventArgs args ) {
            SendQuickPollSheet( Group.AllParticipant );
        }

        private void SendQuickPollSheet( Group receivers ) {
            this.Sender.Post( delegate() {
                this.SendQuickPollSheetHelper( receivers );
            } );
        }

        private void SendQuickPollSheetHelper( Group receivers ) {
            //Don't send message if it's an instructor note.
            if( this.sheet.Disposition != SheetDisposition.Instructor ) {
                Message message, deck, poll, slide;
                message = new PresentationInformationMessage( this.Presentation );
                message.Group = receivers;
                message.InsertChild( deck = new DeckInformationMessage( this.Deck ) );
                deck.InsertChild( slide = new SlideInformationMessage( this.Slide ) );
                using( Synchronizer.Lock( this.Sheet.SyncRoot ) ) {
                    slide.InsertChild( poll = new QuickPollInformationMessage( this.sheet.QuickPoll ) );
                }
                poll.InsertChild( new QuickPollSheetMessage( this.sheet, this.SheetCollectionSelector ) );
                using( Synchronizer.Lock( m_SlideModel.SyncRoot ) ) {
                    message.Tags = new MessageTags();
                    message.Tags.SlideID = m_SlideModel.Id;
                }
                this.Sender.Send( message );
            }
        }
        protected override void Dispose( bool disposing ) {
            if( this.m_Disposed ) return;
            base.Dispose( disposing );
            this.m_Disposed = true;
        }
        public override void ForceUpdate( UW.ClassroomPresenter.Network.Groups.Group receivers ) {
            this.SendQuickPollSheet( receivers );
        }
    }
}
